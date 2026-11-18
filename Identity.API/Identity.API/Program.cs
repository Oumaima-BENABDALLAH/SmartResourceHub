using Identity.Infrastructure;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// Save EF Core + Identity + JWT + AuthService
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// ORDRE IMPORTANT : Authentication avant Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();