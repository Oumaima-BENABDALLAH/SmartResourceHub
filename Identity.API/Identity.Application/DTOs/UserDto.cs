using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Application.DTOs
{
    // Représentation publique d'un utilisateur
    // Ne contient JAMAIS le mot de passe ou le hash
    public sealed record UserDto(
        string Id,
        string Email,
        string FirstName,
        string LastName,
        string FullName,
        IList<string> Roles,
        DateTime CreatedAt
    );
}
