using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le titre est obligatoire")]
        [StringLength(100)]
        public string Titre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La description est obligatoire")]
        public string Description { get; set; } = string.Empty;

        public StatutTicket Statut { get; set; } = StatutTicket.Ouvert;
        public PrioriteTicket Priorite { get; set; } = PrioriteTicket.Moyenne;
        public CategorieTicket Categorie { get; set; } = CategorieTicket.Autre;

        public DateTime DateCreation { get; set; } = DateTime.Now;
        public DateTime? DateMiseAJour { get; set; }

        // Résultats de l'analyse IA
        public string? AnalyseIA_Categorie { get; set; }
        public string? AnalyseIA_Priorite { get; set; }
        public string? AnalyseIA_Suggestion { get; set; }

        // Relation : utilisateur qui a créé le ticket
        public string? CreateurId { get; set; }
        public ApplicationUser? Createur { get; set; }

        // Relation : technicien assigné (null si pas encore assigné)
        public string? TechnicienId { get; set; }
        public ApplicationUser? Technicien { get; set; }

        // Commentaires liés à ce ticket
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}