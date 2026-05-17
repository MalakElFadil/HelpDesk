using HelpDesk.Interfaces;
using System.Text;
using System.Text.Json;

namespace HelpDesk.Services
{
    public class AIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public AIService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GroqApiKey"] ?? string.Empty;

            // Vérification au démarrage
            Console.WriteLine($"[AI] Clé Groq chargée : {(_apiKey.Length > 5 ? "OUI (" + _apiKey.Substring(0, 5) + "...)" : "NON — clé vide")}");
        }

        public async Task<AIAnalysisResult?> AnalyserTicketAsync(
            string titre, string description)
        {
            // Si pas de clé → mode dégradé, on retourne null
            if (string.IsNullOrEmpty(_apiKey))
                return null;

            try
            {
                // Prompt envoyé à Groq — on demande une réponse JSON uniquement
                var prompt = $$"""
                    Analyse ce ticket de support informatique.
                    Réponds UNIQUEMENT en JSON, sans texte avant ou après.

                    Titre : {{titre}}
                    Description : {{description}}

                    Format de réponse attendu :
                    {
                        "categorie": "Reseau|Materiel|Logiciel|AccesCompte|Autre",
                        "priorite": "Basse|Moyenne|Haute",
                        "suggestion": "Une phrase de suggestion de résolution."
                    }
                    """;

                // Construction du corps de la requête Groq
                var requestBody = new
                {
                    model = "llama-3.3-70b-versatile",
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 300,
                    temperature = 0.3
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(
                    json, Encoding.UTF8, "application/json");

                // Ajout du header Authorization
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add(
                    "Authorization", $"Bearer {_apiKey}");

                // Appel à l'API Groq
                var response = await _httpClient.PostAsync(
                    "https://api.groq.com/openai/v1/chat/completions", content);

                // Log du résultat
                Console.WriteLine($"[AI] Status: {(int)response.StatusCode}");
                if (!response.IsSuccessStatusCode)
                {
                    var errBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[AI] Erreur Groq: {errBody}");
                    return null;
                }

                // Lecture de la réponse
                var responseJson = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(responseJson);

                // Extraire le contenu du message retourné par l'IA
                var messageContent = doc
                    .RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (string.IsNullOrEmpty(messageContent))
                    return null;

                // Parser le JSON retourné par l'IA
                var result = JsonSerializer.Deserialize<AIAnalysisResult>(
                    messageContent,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                return result;
            }
            catch (Exception ex)
            {
                // Afficher l'erreur exacte dans la console Visual Studio
                Console.WriteLine($"[AI ERROR] {ex.GetType().Name}: {ex.Message}");
                return null;
            }
        }
    }
}