namespace HelpDesk.Interfaces
{
    // Résultat retourné par l'analyse IA
    public class AIAnalysisResult
    {
        public string Categorie { get; set; } = string.Empty;
        public string Priorite { get; set; } = string.Empty;
        public string Suggestion { get; set; } = string.Empty;
    }

    // Contrat du service IA
    public interface IAIService
    {
        Task<AIAnalysisResult?> AnalyserTicketAsync(string titre, string description);
    }
}