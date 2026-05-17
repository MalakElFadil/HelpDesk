using HelpDesk.Models;

namespace HelpDesk.Interfaces
{
    // Contrat que doit respecter tout repository de tickets
    public interface ITicketRepository
    {
        // Récupérer tous les tickets
        Task<IEnumerable<Ticket>> GetAllAsync();

        // Récupérer les tickets d'un utilisateur spécifique
        Task<IEnumerable<Ticket>> GetByUserIdAsync(string userId);

        // Récupérer les tickets assignés à un technicien
        Task<IEnumerable<Ticket>> GetByTechnicienIdAsync(string technicienId);

        // Récupérer un ticket par son Id
        Task<Ticket?> GetByIdAsync(int id);

        // Ajouter un nouveau ticket
        Task AddAsync(Ticket ticket);

        // Modifier un ticket existant
        Task UpdateAsync(Ticket ticket);

        // Supprimer un ticket
        Task DeleteAsync(int id);
    }
}