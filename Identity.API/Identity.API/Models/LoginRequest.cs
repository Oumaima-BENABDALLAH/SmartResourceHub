namespace Identity.API.Models
{
    // Données envoyées par le client pour se connecter
    public sealed record LoginRequest(
        string Email,
        string Password
    );
}
