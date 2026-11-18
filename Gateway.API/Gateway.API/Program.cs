using Gateway.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

// Désactiver le remapping automatique des claims JWT
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

// 1. YARP  Reverse Proxy Charge la configuration depuis appsettings.json (section ReverseProxy)

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// 2. Redis — pour JWT blacklist + Rate limiting Singleton car une seule connexion TCP partagée

var redisConnection = builder.Configuration.GetConnectionString("Redis")
    ?? "localhost:6379,abortConnect=false";

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConnection));

// 3. JWT Authentication Gateway valide le token avant de router la requête

var jwtSection = builder.Configuration.GetSection("Jwt");
var secret = jwtSection["Secret"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// 4. Authorization policies "authenticated" → routes protégées (Catalog, Booking) "anonymous" → routes publiques (Identity)

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("authenticated", policy =>
        policy.RequireAuthenticatedUser())
    .AddPolicy("Public", policy =>
        policy.RequireAssertion(_ => true)); // Toujours autorisé

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors();

// ORDRE CRITIQUE du pipeline :
// 1. Rate Limiting (bloquer les abus avant tout)
// 2. JWT Validation (vérifier l'identité)
// 3. YARP (router vers le bon service)

app.UseMiddleware<RateLimitingMiddleware>();
app.UseMiddleware<JwtValidationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// YARP gère le routing pas besoin de MapControllers()
app.MapReverseProxy();

app.Run();