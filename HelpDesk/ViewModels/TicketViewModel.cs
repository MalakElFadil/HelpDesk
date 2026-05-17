using HelpDesk.Models;
using System.ComponentModel.DataAnnotations;

namespace HelpDesk.ViewModels
{
    public class CreateTicketViewModel
    {
        [Required(ErrorMessage = "Le titre est obligatoire")]
        [StringLength(200, MinimumLength = 5,
            ErrorMessage = "Le titre doit contenir entre 5 et 200 caractères")]
        [Display(Name = "Titre")]
        public required string Title { get; set; }

        [Required(ErrorMessage = "La description est obligatoire")]
        [StringLength(2000, MinimumLength = 10,
            ErrorMessage = "La description doit contenir entre 10 et 2000 caractères")]
        [Display(Name = "Description")]
        public required string Description { get; set; }

        [Display(Name = "Catégorie")]
        public CategorieTicket Category { get; set; } = CategorieTicket.Autre;

        [Display(Name = "Priorité")]
        public PrioriteTicket Priority { get; set; } = PrioriteTicket.Moyenne;

        [Display(Name = "Pièce jointe")]
        public IFormFile? Attachment { get; set; }
    }

    public class TicketListViewModel
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public CategorieTicket Category { get; set; }
        public StatutTicket Status { get; set; }
        public PrioriteTicket Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public required string CreatedByName { get; set; }
        public string? AssignedToName { get; set; }
    }

    public class TicketDetailViewModel
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public CategorieTicket Category { get; set; }
        public StatutTicket Status { get; set; }
        public PrioriteTicket Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public required string CreatedByName { get; set; }
        public string? AssignedToName { get; set; }
        public string? AIAnalysis { get; set; }
        public List<CommentViewModel> Comments { get; set; } = new();
        public string? NewComment { get; set; }
    }

    public class CommentViewModel
    {
        public int Id { get; set; }
        public required string Content { get; set; }
        public required string AuthorName { get; set; }
        public string? AuthorRole { get; set; }
        public bool IsCurrentUser { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TechnicianDashboardViewModel
    {
        public int TotalAssigned { get; set; }
        public int EnCours { get; set; }
        public int Resolus { get; set; }
        public int Urgents { get; set; }
        public List<TicketListViewModel> TicketsRecents { get; set; } = new();
    }
}