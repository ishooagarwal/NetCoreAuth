using AspNetNull.Persistance.Data;
using AspNetNull.Persistance.Models.Application;
using AspNetNull.Persistance.Repository;
using AspNetNull.Persistance.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetNull.Persistance.Extensions
{
    public static class ServiceLevelExtension
    {
        public static IServiceCollection RegisterDBContext(this IServiceCollection service, IConfiguration configuration)
        {
            service.AddDbContext<ApplicationDBContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection")));
            
            service.AddIdentity<ApplicationUser, ApplicationRole>(options => {
                options.User.RequireUniqueEmail = true;
            })
                .AddEntityFrameworkStores<ApplicationDBContext>()
                .AddDefaultTokenProviders();

            return service;
        }

        public static IServiceCollection RegisterRepositories(this IServiceCollection service)
        {
            service.AddScoped<IUnitOfWork, UnitOfWork>();
            return service;
        }
    }
}
