using HelpDesk.Models;
using Microsoft.AspNetCore.Identity;

namespace HelpDesk.Data
{
    public static class DbInitializer
    {
        // Crée les rôles et les comptes de test au premier démarrage
        public static async Task SeedAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // 1. Créer les 3 rôles si ils n'existent pas
            string[] roles = ["Administrateur", "Technicien", "Utilisateur"];
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // 2. Compte Admin
            await CreateUser(userManager,
                email: "admin@helpdesk.ma",
                password: "admin123",
                nom: "Système", prenom: "Admin",
                role: "Administrateur");

            // 3. Compte Technicien
            await CreateUser(userManager,
                email: "tech@helpdesk.ma",
                password: "tech123",
                nom: "Khalid", prenom: "Technicien",
                role: "Technicien");

            // 4. Compte Utilisateur
            await CreateUser(userManager,
                email: "user@helpdesk.ma",
                password: "user123",
                nom: "Benchekroun", prenom: "Karim",
                role: "Utilisateur");
        }

        // Méthode utilitaire pour créer un utilisateur
        private static async Task CreateUser(
            UserManager<ApplicationUser> userManager,
            string email, string password,
            string nom, string prenom, string role)
        {
            // Vérifier si l'utilisateur existe déjà
            if (await userManager.FindByEmailAsync(email) != null)
                return;

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                Nom = nom,
                Prenom = prenom,
                EstActif = true,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, role);
        }
    }
}