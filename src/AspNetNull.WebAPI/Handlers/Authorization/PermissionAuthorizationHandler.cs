using AspNetNull.WebAPI.Handlers.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace AspNetNull.WebAPI.Handlers.Authorization
{
    public class PermissionAuthorizationHandler : BaseAuthorizationHandler<PermissionRequirements>
    {
        private IConfiguration Configuration { get; }

        public PermissionAuthorizationHandler(IConfiguration configuration)
            : base(configuration)
        {
            this.Configuration = configuration;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirements requirement)
        {
            foreach (var claim in requirement.Claims)
            {
                base.ClaimType = claim.Name;
                base.HandleRequirementAsync(context, requirement);
            }

            return Task.CompletedTask;
        }
    }
}
