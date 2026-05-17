using HelpDesk.Models;

namespace HelpDesk.Interfaces
{
    // Contrat du service métier tickets
    public interface ITicketService
    {
        // Récupérer tous les tickets (pour Admin)
        Task<IEnumerable<Ticket>> GetAllTicketsAsync();

        // Récupérer les tickets d'un utilisateur (pour User)
        Task<IEnumerable<Ticket>> GetUserTicketsAsync(string userId);

        // Récupérer les tickets assignés à un technicien
        Task<IEnumerable<Ticket>> GetTechnicienTicketsAsync(string technicienId);

        // Récupérer un ticket par son Id
        Task<Ticket?> GetTicketByIdAsync(int id);

        // Créer un ticket + déclencher l'analyse IA
        Task<Ticket> CreateTicketAsync(Ticket ticket, string createurId);

        // Mettre à jour le statut d'un ticket
        Task UpdateStatutAsync(int ticketId, StatutTicket nouveauStatut);

        // Ajouter un commentaire à un ticket
        Task AddCommentAsync(int ticketId, string contenu, string auteurId);

        // Assigner un technicien à un ticket (Admin)
        Task AssignerTechnicienAsync(int ticketId, string technicienId);

        // Supprimer un ticket (Admin)
        Task DeleteTicketAsync(int id);
    }
}