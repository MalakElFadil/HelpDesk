using HelpDesk.Data;
using HelpDesk.Models;
using HelpDesk.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Controllers
{
    [Authorize(Roles = "Technicien")]
    public class TechnicianController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TechnicianController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Technician/Dashboard
        public IActionResult Dashboard()
        {
            // Récupérer uniquement les tickets assignés au technicien connecté
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var tickets = _context.Tickets
                .Include(t => t.Createur)
                .Include(t => t.Technicien)
                .Where(t => t.TechnicienId == userId) // ← filtre par technicien
                .ToList();

            var vm = new TechnicianDashboardViewModel
            {
                TotalAssigned = tickets.Count,
                EnCours = tickets.Count(t => t.Statut == StatutTicket.EnCours),
                Resolus = tickets.Count(t => t.Statut == StatutTicket.Resolu),
                Urgents = tickets.Count(t => t.Priorite == PrioriteTicket.Haute),
                TicketsRecents = tickets
                    .OrderByDescending(t => t.DateCreation)
                    .Take(10)
                    .Select(t => new TicketListViewModel
                    {
                        Id = t.Id,
                        Title = t.Titre,
                        Category = t.Categorie,
                        Status = t.Statut,
                        Priority = t.Priorite,
                        CreatedAt = t.DateCreation,
                        CreatedByName = t.Createur != null
                                         ? $"{t.Createur.Prenom} {t.Createur.Nom}"
                                         : "Inconnu",
                        AssignedToName = t.Technicien != null
                                         ? $"{t.Technicien.Prenom} {t.Technicien.Nom}"
                                         : null
                    })
                    .ToList()
            };

            return View(vm);
        }

        // GET: /Technician/Detail/5
        // GET: /Technician/Detail/5
        public IActionResult Detail(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var ticket = _context.Tickets
                .Include(t => t.Createur)
                .Include(t => t.Technicien)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.Auteur)  // ← charge l'auteur de chaque commentaire
                .FirstOrDefault(t => t.Id == id);

            if (ticket == null)
                return NotFound();

            // Sécurité : seul le technicien assigné peut voir ce ticket
            if (ticket.TechnicienId != userId)
                return Forbid();

            var comments = ticket.Comments
                .OrderBy(c => c.DateCreation)
                .Select(c => new CommentViewModel
                {
                    Id = c.Id,
                    Content = c.Contenu,
                    IsCurrentUser = c.AuteurId == userId,
                    // "Moi" si c'est le technicien connecté, sinon nom + prénom
                    AuthorName = c.AuteurId == userId
                                    ? "Moi"
                                    : (c.Auteur != null
                                       ? $"{c.Auteur.Prenom} {c.Auteur.Nom}"
                                       : "Inconnu"),
                    // Rôle simple selon si c'est le créateur ou le technicien
                    AuthorRole = c.AuteurId == ticket.CreateurId
                                    ? "Utilisateur"
                                    : "Technicien",
                    CreatedAt = c.DateCreation
                })
                .ToList();

            string? analyseIA = null;
            if (!string.IsNullOrEmpty(ticket.AnalyseIA_Suggestion))
            {
                analyseIA = $"Catégorie suggérée : {ticket.AnalyseIA_Categorie}\n" +
                            $"Priorité suggérée  : {ticket.AnalyseIA_Priorite}\n" +
                            $"Suggestion         : {ticket.AnalyseIA_Suggestion}";
            }

            var vm = new TicketDetailViewModel
            {
                Id = ticket.Id,
                Title = ticket.Titre,
                Description = ticket.Description,
                Category = ticket.Categorie,
                Status = ticket.Statut,
                Priority = ticket.Priorite,
                CreatedAt = ticket.DateCreation,
                CreatedByName = ticket.Createur != null
                                 ? $"{ticket.Createur.Prenom} {ticket.Createur.Nom}" : "Inconnu",
                AssignedToName = ticket.Technicien != null
                                 ? $"{ticket.Technicien.Prenom} {ticket.Technicien.Nom}" : null,
                AIAnalysis = analyseIA,
                Comments = comments
            };

            return View(vm);
        }

        // POST: /Technician/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus(int ticketId, StatutTicket statut)
        {
            var ticket = _context.Tickets
                .FirstOrDefault(t => t.Id == ticketId);

            if (ticket == null)
                return NotFound();

            ticket.Statut = statut;
            ticket.DateMiseAJour = DateTime.Now;
            _context.SaveChanges();

            return RedirectToAction("Detail", new { id = ticketId });
        }

        // POST: /Technician/Reply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reply(int ticketId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return RedirectToAction("Detail", new { id = ticketId });

            // Récupérer l'id du technicien connecté
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var comment = new Comment
            {
                TicketId = ticketId,
                Contenu = content,
                AuteurId = userId,   // ← l'auteur est le technicien connecté
                DateCreation = DateTime.Now
            };

            _context.Comments.Add(comment);
            _context.SaveChanges();

            return RedirectToAction("Detail", new { id = ticketId });
        }
    }
}