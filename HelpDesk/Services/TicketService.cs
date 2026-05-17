using HelpDesk.Interfaces;
using HelpDesk.Models;

namespace HelpDesk.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IAIService _aiService;

        // On injecte le repository et le service IA via le constructeur
        public TicketService(ITicketRepository ticketRepository, IAIService aiService)
        {
            _ticketRepository = ticketRepository;
            _aiService = aiService;
        }

        // Récupérer tous les tickets
        public async Task<IEnumerable<Ticket>> GetAllTicketsAsync()
        {
            return await _ticketRepository.GetAllAsync();
        }

        // Récupérer les tickets d'un utilisateur
        public async Task<IEnumerable<Ticket>> GetUserTicketsAsync(string userId)
        {
            return await _ticketRepository.GetByUserIdAsync(userId);
        }

        // Récupérer les tickets d'un technicien
        public async Task<IEnumerable<Ticket>> GetTechnicienTicketsAsync(string technicienId)
        {
            return await _ticketRepository.GetByTechnicienIdAsync(technicienId);
        }

        // Récupérer un ticket par Id
        public async Task<Ticket?> GetTicketByIdAsync(int id)
        {
            return await _ticketRepository.GetByIdAsync(id);
        }

        // Créer un ticket + appeler l'IA automatiquement
        public async Task<Ticket> CreateTicketAsync(Ticket ticket, string createurId)
        {
            // Associer le créateur
            ticket.CreateurId = createurId;
            ticket.DateCreation = DateTime.Now;
            ticket.Statut = StatutTicket.Ouvert;

            // Sauvegarder d'abord pour obtenir l'Id
            await _ticketRepository.AddAsync(ticket);

            // Appeler l'IA pour analyser le ticket
            try
            {
                var analyse = await _aiService.AnalyserTicketAsync(
                    ticket.Titre, ticket.Description);

                if (analyse != null)
                {
                    // Mettre à jour avec les résultats de l'IA
                    ticket.AnalyseIA_Categorie = analyse.Categorie;
                    ticket.AnalyseIA_Priorite = analyse.Priorite;
                    ticket.AnalyseIA_Suggestion = analyse.Suggestion;
                    await _ticketRepository.UpdateAsync(ticket);
                }
            }
            catch
            {
                // Mode dégradé : si l'IA échoue, le ticket est quand même créé
            }

            return ticket;
        }

        // Mettre à jour le statut
        public async Task UpdateStatutAsync(int ticketId, StatutTicket nouveauStatut)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null) return;

            ticket.Statut = nouveauStatut;
            ticket.DateMiseAJour = DateTime.Now;
            await _ticketRepository.UpdateAsync(ticket);
        }

        // Ajouter un commentaire
        public async Task AddCommentAsync(int ticketId, string contenu, string auteurId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null) return;

            var comment = new Comment
            {
                TicketId = ticketId,
                Contenu = contenu,
                AuteurId = auteurId,
                DateCreation = DateTime.Now
            };

            ticket.Comments.Add(comment);
            await _ticketRepository.UpdateAsync(ticket);
        }

        // Assigner un technicien à un ticket
        public async Task AssignerTechnicienAsync(int ticketId, string technicienId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null) return;

            ticket.TechnicienId = technicienId;
            ticket.Statut = StatutTicket.EnCours;
            ticket.DateMiseAJour = DateTime.Now;
            await _ticketRepository.UpdateAsync(ticket);
        }

        // Supprimer un ticket
        public async Task DeleteTicketAsync(int id)
        {
            await _ticketRepository.DeleteAsync(id);
        }
    }
}