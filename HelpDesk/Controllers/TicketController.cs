using HelpDesk.Interfaces;
using HelpDesk.Models;
using HelpDesk.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Controllers
{
    // Toutes les actions nécessitent une connexion
    [Authorize]
    public class TicketController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TicketController(
            ITicketService ticketService,
            UserManager<ApplicationUser> userManager)
        {
            _ticketService = ticketService;
            _userManager = userManager;
        }

        // GET /Ticket/Index — liste des tickets de l'utilisateur connecté
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var tickets = await _ticketService.GetUserTicketsAsync(userId);

            // Convertir chaque ticket en ViewModel pour la vue
            var viewModels = tickets.Select(t => new TicketListViewModel
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

            return View(viewModels);
        }

        // GET /Ticket/Create — afficher le formulaire de création
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateTicketViewModel
            {
                Title = string.Empty,
                Description = string.Empty
            });
        }

        // POST /Ticket/Create — soumettre le formulaire
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTicketViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = _userManager.GetUserId(User)!;

            // Créer l'entité Ticket depuis le ViewModel
            var ticket = new Ticket
            {
                Titre = model.Title,
                Description = model.Description,
                Priorite = model.Priority,
                Categorie = model.Category,
            };

            // Créer le ticket + analyse IA automatique
            var created = await _ticketService.CreateTicketAsync(ticket, userId);

            TempData["Success"] = "Ticket créé avec succès !";
            return RedirectToAction("Detail", new { id = created.Id });
        }

        // GET /Ticket/Detail/5 — détail d'un ticket
        public async Task<IActionResult> Detail(int id)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket == null)
                return NotFound();

            // Construire le ViewModel avec toutes les infos du ticket
            var vm = new TicketDetailViewModel
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
                AssignedToName = ticket.Technicien != null
                                 ? $"{ticket.Technicien.Prenom} {ticket.Technicien.Nom}"
                                 : null,
                // Afficher la suggestion IA si disponible
                AIAnalysis = ticket.AnalyseIA_Suggestion,
                Comments = ticket.Comments.Select(c => new CommentViewModel
                {
                    Id = c.Id,
                    Content = c.Contenu,
                    AuthorName = c.Auteur != null
                                 ? $"{c.Auteur.Prenom} {c.Auteur.Nom}"
                                 : "Inconnu",
                    CreatedAt = c.DateCreation
                }).ToList()
            };

            return View(vm);
        }

        // POST /Ticket/AddComment — ajouter un commentaire
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int ticketId, string content)
        {
            if (!string.IsNullOrWhiteSpace(content))
            {
                var userId = _userManager.GetUserId(User)!;
                await _ticketService.AddCommentAsync(ticketId, content, userId);
            }

            return RedirectToAction("Detail", new { id = ticketId });
        }
    }
}