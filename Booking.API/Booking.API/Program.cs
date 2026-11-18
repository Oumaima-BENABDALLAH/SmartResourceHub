// Program.cs Configure le pipeline des requêtes HTTP (UseHttpsRedirection, MapControllers).
using Booking.Application;
using Booking.Infrastructure;
using Booking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

///WebApplication.CreateBuilder(args) configurer les logs et gérer le conteneur d'injection de dépendances (builder.Services).
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers(); //Enregistre les contrôleurs

builder.Services.AddApplication(); //Méthode d'extension personnalisée qui enregistre tous les composants de ta couche Application (MediatR, les gestionnaires/handlers, les validateurs FluentValidation).
builder.Services.AddInfrastructure(builder.Configuration); //enregistre les services techniques de la couche Infrastructure (la base de données SQL Server avec le DbContext, les clients gRPC, les brokers RabbitMQ)

var app = builder.Build(); //Finalise la configuration et construit l'application (app).

// Applique automatiquement les migrations au démarrage (dev) ,ce bloc vérifie la base de données et applique automatiquement les migrations en attente , pas besoin de taper la commande dotnet ef database update
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
        db.Database.Migrate();
    }
}


// Vérifie si tu es en train de travailler en local (sur ton PC). Si c'est le cas, il active la documentation de l'API (OpenApi / Swagger).
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//Redirige automatiquement toutes les requêtes HTTP non sécurisées vers du HTTPS sécurisé.
app.UseHttpsRedirection();

//Indique à .NET d'écouter les routes de tes contrôleurs (pour que api/bookings fonctionne).
app.MapControllers();

//Démarre le serveur web et met l'application en écoute des requêtes entrantes
app.Run();