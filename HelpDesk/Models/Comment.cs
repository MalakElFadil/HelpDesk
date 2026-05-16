using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string Contenu { get; set; } = string.Empty;

        public DateTime DateCreation { get; set; } = DateTime.Now;

        // Relation : ticket auquel appartient ce commentaire
        public int TicketId { get; set; }
        public Ticket? Ticket { get; set; }

        // Relation : auteur du commentaire
        public string? AuteurId { get; set; }
        public ApplicationUser? Auteur { get; set; }
    }
}