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
        // GET /Ticket/Index — liste des tickets avec filtre optionnel par statut
        public async Task<IActionResult> Index(string? status)
        {
            var userId = _userManager.GetUserId(User)!;
            var tickets = await _ticketService.GetUserTicketsAsync(userId);

            // Appliquer le filtre si un statut est sélectionné
            if (!string.IsNullOrEmpty(status) &&
                Enum.TryParse<StatutTicket>(status, out var statutEnum))
            {
                tickets = tickets.Where(t => t.Statut == statutEnum);
            }

            var viewModels = tickets.Select(t => new TicketListViewModel
            {
                Id = t.Id,
                Title = t.Titre,
                Category = t.Categorie,
                Priority = t.Priorite,
                Status = t.Statut,
                CreatedAt = t.DateCreation,
                CreatedByName = t.Createur != null
                                 ? $"{t.Createur.Prenom} {t.Createur.Nom}" : "Inconnu",
                AssignedToName = t.Technicien != null
                                 ? $"{t.Technicien.Prenom} {t.Technicien.Nom}" : null
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
            // Debug temporaire — afficher les erreurs dans la console
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                foreach (var error in state!.Errors)
                {
                    Console.WriteLine($"[VALIDATION ERROR] {key}: {error.ErrorMessage}");
                }
            }

            // Afficher les erreurs de validation dans la vue
            if (!ModelState.IsValid)
            {
                // Passer les erreurs à la vue pour les voir
                foreach (var error in ModelState.Values
                                                .SelectMany(v => v.Errors))
                {
                    ModelState.AddModelError(string.Empty, error.ErrorMessage);
                }
                return View(model);
            }

            var userId = _userManager.GetUserId(User)!;

            var ticket = new Ticket
            {
                Titre = model.Title,
                Description = model.Description,
                Priorite = model.Priority,
                Categorie = model.Category,
            };

            var created = await _ticketService.CreateTicketAsync(ticket, userId);

            TempData["Success"] = "Ticket créé avec succès !";
            return RedirectToAction("Detail", new { id = created.Id });
        }

        // GET /Ticket/Detail/5 — détail d'un ticket
        // GET /Ticket/Detail/5 — détail d'un ticket
        public async Task<IActionResult> Detail(int id)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket == null)
                return NotFound();

            // Id de l'utilisateur connecté — pour afficher "Moi"
            var currentUserId = _userManager.GetUserId(User);

            // ── Construire les commentaires avec rôle ──────────────────────
            var comments = new List<CommentViewModel>();
            foreach (var c in ticket.Comments.OrderBy(c => c.DateCreation))
            {
                // Récupérer le rôle de l'auteur via UserManager
                string role = "";
                if (c.Auteur != null)
                {
                    var roles = await _userManager.GetRolesAsync(c.Auteur);
                    role = roles.FirstOrDefault() switch
                    {
                        "Administrateur" => "Admin",
                        "Technicien" => "Technicien",
                        _ => "Utilisateur"
                    };
                }

                comments.Add(new CommentViewModel
                {
                    Id = c.Id,
                    Content = c.Contenu,
                    IsCurrentUser = c.AuteurId == currentUserId,
                    AuthorName = c.AuteurId == currentUserId
                                    ? "Moi"
                                    : (c.Auteur != null
                                       ? $"{c.Auteur.Prenom} {c.Auteur.Nom}"
                                       : "Inconnu"),
                    AuthorRole = role,
                    CreatedAt = c.DateCreation
                });
            }
            // ──────────────────────────────────────────────────────────────

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
                AIAnalysis = ticket.AnalyseIA_Suggestion,
                Comments = comments   // ← les commentaires avec rôle
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