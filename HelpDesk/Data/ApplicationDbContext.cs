using HelpDesk.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Data
{
    // Hérite de IdentityDbContext pour avoir les tables Identity automatiquement
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Un ticket est créé par un utilisateur
            builder.Entity<Ticket>()
                .HasOne(t => t.Createur)
                .WithMany(u => u.TicketsCrees)
                .HasForeignKey(t => t.CreateurId)
                .OnDelete(DeleteBehavior.Restrict); // Empeche de supprimer un utilisateur qui a créer un ticket

            // Un ticket peut être assigné à un technicien
            builder.Entity<Ticket>()
                .HasOne(t => t.Technicien)
                .WithMany(u => u.TicketsAssignes)
                .HasForeignKey(t => t.TechnicienId)
                .OnDelete(DeleteBehavior.Restrict); // Empeche de supprimer un utilisateur qui a assigné un ticket
        }
    }
}