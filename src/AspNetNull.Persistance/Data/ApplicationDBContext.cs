using AspNetNull.Persistance.Models.Application;
using AspNetNull.Persistance.Models.Claims.Requests;
using AspNetNull.Persistance.Models.Policy.Requests;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AspNetNull.Persistance.Data
{
    public class ApplicationDBContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
            : base(options)
        {
        }

        public DbSet<PolicyClaimsMappingRequest> PolicyClamisMappings { get; set; }
        public DbSet<ClaimRequest> Claims { get; set; }
    }
}
