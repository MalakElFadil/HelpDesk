using HelpDesk.Data;
using HelpDesk.Interfaces;
using HelpDesk.Models;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Data.Repositories
{
    // Implémentation concrète avec Entity Framework Core
    public class TicketRepository : ITicketRepository
    {
        private readonly ApplicationDbContext _context;

        // On injecte le DbContext via le constructeur
        public TicketRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Récupérer tous les tickets avec les infos du créateur et du technicien
        public async Task<IEnumerable<Ticket>> GetAllAsync()
        {
            return await _context.Tickets
                .Include(t => t.Createur)
                .Include(t => t.Technicien)
                .Include(t => t.Comments)
                .OrderByDescending(t => t.DateCreation)
                .ToListAsync();
        }

        // Récupérer uniquement les tickets créés par un utilisateur donné
        public async Task<IEnumerable<Ticket>> GetByUserIdAsync(string userId)
        {
            return await _context.Tickets
                .Include(t => t.Createur)
                .Include(t => t.Technicien)
                .Include(t => t.Comments)
                .Where(t => t.CreateurId == userId)
                .OrderByDescending(t => t.DateCreation)
                .ToListAsync();
        }

        // Récupérer les tickets assignés à un technicien donné
        public async Task<IEnumerable<Ticket>> GetByTechnicienIdAsync(string technicienId)
        {
            return await _context.Tickets
                .Include(t => t.Createur)
                .Include(t => t.Technicien)
                .Include(t => t.Comments)
                .Where(t => t.TechnicienId == technicienId)
                .OrderByDescending(t => t.DateCreation)
                .ToListAsync();
        }

        // Récupérer un ticket par son Id
        public async Task<Ticket?> GetByIdAsync(int id)
        {
            return await _context.Tickets
                .Include(t => t.Createur)
                .Include(t => t.Technicien)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.Auteur)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        // Ajouter un nouveau ticket dans la base
        public async Task AddAsync(Ticket ticket)
        {
            await _context.Tickets.AddAsync(ticket);
            await _context.SaveChangesAsync();
        }

        // Mettre à jour un ticket existant
        public async Task UpdateAsync(Ticket ticket)
        {
            ticket.DateMiseAJour = DateTime.Now;
            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();
        }

        // Supprimer un ticket par son Id
        public async Task DeleteAsync(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();
            }
        }
    }
}