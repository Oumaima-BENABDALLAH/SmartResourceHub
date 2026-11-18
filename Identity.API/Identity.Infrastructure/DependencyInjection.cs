using Identity.Application.Interfaces;
using Identity.Application.Services;
using Identity.Domain.Entities;
using Identity.Infrastructure.Cache;
using Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            // 1 :  EF core : connexion à SQL Server
           services.AddDbContext<IdentityAppDbContext>(options =>
           options.UseSqlServer(
               configuration.GetConnectionString("IdentityDb"),
               b => b.MigrationsAssembly(typeof(IdentityAppDbContext).Assembly.FullName)));

            // 2:  ASP.NET Core Identity

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Configuration du mot de passe
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
         .AddEntityFrameworkStores<IdentityAppDbContext>()
         .AddDefaultTokenProviders(); // Nécessaire pour les tokens de reset password


            // 3. Authentication JWT
            // On configure comment valider les tokens reçus dans les requêtes

            var jwtSection = configuration.GetSection("Jwt");
            var secret = jwtSection["Secret"]
                ?? throw new InvalidOperationException("JWT Secret is not configured.");

            services.AddAuthentication(options =>
            {
                // Par défaut, toutes les requêtes sont validées via JWT
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Valider que le token vient bien de serveur
                    ValidateIssuer = true,
                    ValidIssuer = jwtSection["Issuer"],

                    // Valider que le token est destiné à notre API
                    ValidateAudience = true,
                    ValidAudience = jwtSection["Audience"],

                    // Valider la signature avec notre clé secrète
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(secret)),

                    // Valider que le token n'est pas expiré
                    ValidateLifetime = true,

                    // Tolérance d'horloge (évite les problèmes de décalage)
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"[DEBUG] Auth Failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    }
                };
            });
            // 4. Authorization
            services.AddAuthorization();

            // 5. Redis  : IConnectionMultiplexer en Singleton car une seule connexion TCP partagée

            var redisConnection = configuration.GetConnectionString("Redis")
            ?? "localhost:6379";

            var options = ConfigurationOptions.Parse(redisConnection);
            options.AbortOnConnectFail = false; // Force la non-interruption au démarrage
            options.ConnectRetry = 3;           // Tente de se reconnecter 3 fois
            options.ConnectTimeout = 5000;      // Timeout de 5 secondes

            services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(options));

          
            // TokenCacheService en Scoped (une instance par requête HTTP, partage le Singleton Redis)
            services.AddScoped<ITokenCacheService, TokenCacheService>();


            // 5. AuthService — contient la logique Register/Login/GenerateToken
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}
