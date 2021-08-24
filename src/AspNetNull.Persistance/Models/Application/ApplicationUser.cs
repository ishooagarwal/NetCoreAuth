using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AspNetNull.Persistance.Models.Application
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public double Offset { get; set; }
    }
}
