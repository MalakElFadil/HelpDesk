// Ce fichier est l'entrée point de l'application. Il configure les services et le pipeline de traitement des requêtes HTTP.

using HelpDesk.Data;
using HelpDesk.Data.Repositories;
using HelpDesk.Interfaces;
using HelpDesk.Models;
using HelpDesk.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Base de données SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration
        .GetConnectionString("DefaultConnection")));

// 2. Enregistrement des services metiers — injection de dépendances
builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddScoped<ITicketService, TicketService>();

// HttpClient pour l'appel API Groq
builder.Services.AddHttpClient();

// 3. ASP.NET Identity : gestion des comptes et des rôles
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Règles du mot de passe simplifiées pour le développement
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Injection de dépendances — Repositories et Services
builder.Services.AddScoped<HelpDesk.Interfaces.ITicketRepository,
                            HelpDesk.Data.Repositories.TicketRepository>();
builder.Services.AddScoped<HelpDesk.Interfaces.ITicketService,
                            HelpDesk.Services.TicketService>();

// 4. MVC
builder.Services.AddControllersWithViews();

// 5. Rediriger vers /Account/Login si non connecté
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Authentication DOIT être avant Authorization
app.UseAuthentication();
app.UseAuthorization();

// La page d'accueil = page de login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// Initialiser la base de données avec les rôles et comptes de test
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider
        .GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();
    await DbInitializer.SeedAsync(userManager, roleManager);
}

app.Run();