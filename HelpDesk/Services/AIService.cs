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
            // Lire la clé depuis User Secrets
            _apiKey = configuration["GroqApiKey"] ?? string.Empty;
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
                    model = "llama3-70b-8192",
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 200,
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
                    "https://api.groq.com/openai/v1/chat/completions",
                    content);

                if (!response.IsSuccessStatusCode)
                    return null;

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
            catch
            {
                // En cas d'erreur → mode dégradé
                return null;
            }
        }
    }
}