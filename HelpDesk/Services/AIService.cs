using HelpDesk.Interfaces;

namespace HelpDesk.Services
{
    // Implémentation temporaire — sera complétée à la tâche 12
    public class AIService : IAIService
    {
        public Task<AIAnalysisResult?> AnalyserTicketAsync(string titre, string description)
        {
            // Retourne null pour l'instant — mode dégradé
            return Task.FromResult<AIAnalysisResult?>(null);
        }
    }
}