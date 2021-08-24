using AspNetNull.Persistance.Models.UserClaims;
using AspNetNull.Persistance.Repository.IRepository;
using AspNetNull.WebAPI.Handlers.Authorization;
using AspNetNull.WebAPI.Handlers.Requirements;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace AspNetNull.WebAPI.Extension
{
    public static class ServiceLevelExtension
    {
        public static IServiceCollection RegisterAuthorizationPolicies(this IServiceCollection service, IConfiguration configuration)
        {
            IUnitOfWork unitOfWork = service.BuildServiceProvider().GetService<IUnitOfWork>();
            var permissions = unitOfWork.PolicyClaimsMapping.GetAllAsync();

            service.AddAuthorization(options =>
            {
                foreach (var permission in permissions.Result)
                {
                    IList<Claims> claims = Newtonsoft.Json.JsonConvert.DeserializeObject<IList<Claims>>(permission.ClaimsIds.Replace("\r\n", "").Replace("\\", ""));
                    options.AddPolicy(permission.Name, policy =>
                        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                        .RequireAuthenticatedUser()
                        .AddRequirements(new PermissionRequirements(claims))
                        .Build());
                }
            });

            service.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

            return service;
        }

        public static IServiceCollection RegisterDependencies(this IServiceCollection service)
        {
            service.AddAutoMapper(typeof(Mappings.MappingProfiles));

            return service;
        }
    }
}
