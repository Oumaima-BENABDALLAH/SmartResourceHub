using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;


namespace Identity.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        // ApplicationUser étend IdentityUser qui fournit déjà 
        // Id (Guid)
        // Email
        // PasswordHash
        // UserName
        // PhoneNumber
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string FullName => $"{FirstName} {LastName}";

    }
}
