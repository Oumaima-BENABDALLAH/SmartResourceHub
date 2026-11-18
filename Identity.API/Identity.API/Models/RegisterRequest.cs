namespace Identity.API.Models
{
    // Données envoyées par le client pour créer un compte
    public sealed record RegisterRequest
      (string FirstName,
       string LastName,
       string Email,
       string Password);
}
