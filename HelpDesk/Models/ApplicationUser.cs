using Microsoft.AspNetCore.Identity;
using System.Net.Sockets;

namespace HelpDesk.Models
{
    // Hérite de IdentityUser pour ajouter nos propres champs
    public class ApplicationUser : IdentityUser
    {
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public bool EstActif { get; set; } = true;
        public DateTime DateCreation { get; set; } = DateTime.Now;

        // Tickets créés par cet utilisateur
        public ICollection<Ticket> TicketsCrees { get; set; } = new List<Ticket>();

        // Tickets assignés à ce technicien
        public ICollection<Ticket> TicketsAssignes { get; set; } = new List<Ticket>();
    }
}