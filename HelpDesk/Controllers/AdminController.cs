using HelpDesk.Interfaces;
using HelpDesk.Models;
using HelpDesk.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Controllers
{
    // Seul l'Administrateur peut accéder à cet espace
    [Authorize(Roles = "Administrateur")]
    public class AdminController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(
            ITicketService ticketService,
            UserManager<ApplicationUser> userManager)
        {
            _ticketService = ticketService;
            _userManager = userManager;
        }

        // GET /Admin/Dashboard — tableau de bord admin
        public async Task<IActionResult> Dashboard()
        {
            var tickets = await _ticketService.GetAllTicketsAsync();
            var ticketList = tickets.ToList();

            // Calculer les KPIs pour le dashboard
            var vm = new AdminDashboardViewModel
            {
                TotalTickets = ticketList.Count,
                TicketsEnCours = ticketList.Count(t =>
                    t.Statut == StatutTicket.EnCours),
                TicketsResolus = ticketList.Count(t =>
                    t.Statut == StatutTicket.Resolu),
                TicketsUrgents = ticketList.Count(t =>
                    t.Priorite == PrioriteTicket.Haute),

                // Les 5 tickets les plus récents
                TicketsRecents = ticketList
                    .OrderByDescending(t => t.DateCreation)
                    .Take(5)
                    .Select(t => new TicketListViewModel
                    {
                        Id = t.Id,
                        Title = t.Titre,
                        Category = t.Categorie,
                        Priority = t.Priorite,
                        Status = t.Statut,
                        CreatedAt = t.DateCreation,
                        CreatedByName = t.Createur != null
                                         ? $"{t.Createur.Prenom} {t.Createur.Nom}"
                                         : "Inconnu",
                        AssignedToName = t.Technicien != null
                                         ? $"{t.Technicien.Prenom} {t.Technicien.Nom}"
                                         : null
                    }).ToList()
            };

            return View(vm);
        }

        // GET /Admin/Tickets — liste complète de tous les tickets
        public async Task<IActionResult> Tickets()
        {
            var tickets = await _ticketService.GetAllTicketsAsync();

            var vm = tickets.Select(t => new TicketListViewModel
            {
                Id = t.Id,
                Title = t.Titre,
                Category = t.Categorie,
                Priority = t.Priorite,
                Status = t.Statut,
                CreatedAt = t.DateCreation,
                CreatedByName = t.Createur != null
                                 ? $"{t.Createur.Prenom} {t.Createur.Nom}"
                                 : "Inconnu",
                AssignedToName = t.Technicien != null
                                 ? $"{t.Technicien.Prenom} {t.Technicien.Nom}"
                                 : null
            });

            return View(vm);
        }

        // GET /Admin/Detail/5 — détail d'un ticket avec contrôles admin
        public async Task<IActionResult> Detail(int id)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket == null)
                return NotFound();

            // Récupérer la liste des techniciens pour l'assignation
            var techniciens = await _userManager.GetUsersInRoleAsync("Technicien");

            var vm = new AdminTicketDetailViewModel
            {
                Id = ticket.Id,
                Title = ticket.Titre,
                Description = ticket.Description,
                Category = ticket.Categorie,
                Priority = ticket.Priorite,
                Status = ticket.Statut,
                CreatedAt = ticket.DateCreation,
                CreatedByName = ticket.Createur != null
                                 ? $"{ticket.Createur.Prenom} {ticket.Createur.Nom}"
                                 : "Inconnu",
                AssignedToId = ticket.TechnicienId,
                AssignedToName = ticket.Technicien != null
                                 ? $"{ticket.Technicien.Prenom} {ticket.Technicien.Nom}"
                                 : null,
                AIAnalysis = ticket.AnalyseIA_Suggestion,
                Comments = ticket.Comments.Select(c => new CommentViewModel
                {
                    Id = c.Id,
                    Content = c.Contenu,
                    AuthorName = c.Auteur != null
                                 ? $"{c.Auteur.Prenom} {c.Auteur.Nom}"
                                 : "Inconnu",
                    CreatedAt = c.DateCreation
                }).ToList(),

                // Liste des techniciens pour le select
                Techniciens = techniciens.Select(t => new TechnicienViewModel
                {
                    Id = t.Id,
                    Nom = $"{t.Prenom} {t.Nom}"
                }).ToList()
            };

            return View(vm);
        }

        // POST /Admin/AssignerTechnicien
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignerTechnicien(
            int ticketId, string technicienId)
        {
            await _ticketService.AssignerTechnicienAsync(ticketId, technicienId);
            TempData["Success"] = "Technicien assigné avec succès.";
            return RedirectToAction("Detail", new { id = ticketId });
        }

        // POST /Admin/UpdateStatut
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatut(
            int ticketId, StatutTicket statut)
        {
            await _ticketService.UpdateStatutAsync(ticketId, statut);
            TempData["Success"] = "Statut mis à jour.";
            return RedirectToAction("Detail", new { id = ticketId });
        }

        // POST /Admin/DeleteTicket
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTicket(int ticketId)
        {
            await _ticketService.DeleteTicketAsync(ticketId);
            TempData["Success"] = "Ticket supprimé.";
            return RedirectToAction("Tickets");
        }
    }
}