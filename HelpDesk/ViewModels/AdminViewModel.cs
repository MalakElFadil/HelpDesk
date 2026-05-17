using HelpDesk.Models;

namespace HelpDesk.ViewModels
{
    // ── Dashboard Admin ───────────────────────────────────────────────────
    public class AdminDashboardViewModel
    {
        // KPIs
        public int TotalTickets { get; set; }
        public int TicketsEnCours { get; set; }
        public int TicketsResolus { get; set; }
        public int TicketsUrgents { get; set; }

        // 5 tickets les plus récents
        public List<TicketListViewModel> TicketsRecents { get; set; } = new();
    }

    // ── Détail ticket (Admin) ─────────────────────────────────────────────
    public class AdminTicketDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public CategorieTicket? Category { get; set; }
        public PrioriteTicket Priority { get; set; }
        public StatutTicket Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public string? AssignedToId { get; set; }
        public string? AssignedToName { get; set; }
        public string? AIAnalysis { get; set; }
        public List<CommentViewModel> Comments { get; set; } = new();

        // Liste des techniciens disponibles pour l'assignation
        public List<TechnicienViewModel> Techniciens { get; set; } = new();
    }

    // ── Technicien (pour le select d'assignation) ─────────────────────────
    public class TechnicienViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
    }

    // ── Liste utilisateurs ────────────────────────────────────────────────
    public class UserListViewModel
    {
        public required string Id { get; set; }
        public required string Nom { get; set; }
        public required string Prenom { get; set; }
        public required string Email { get; set; }
        public bool EstActif { get; set; }
        public DateTime DateCreation { get; set; }
        public string? Role { get; set; }
    }

    public class UsersPageViewModel
    {
        public List<UserListViewModel> Users { get; set; } = new();
        public string? Filter { get; set; }
    }



}