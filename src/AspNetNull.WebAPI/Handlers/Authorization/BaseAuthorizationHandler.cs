using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace AspNetNull.WebAPI.Handlers.Authorization
{
    public class BaseAuthorizationHandler<TRequirement> : AuthorizationHandler<TRequirement> where TRequirement : IAuthorizationRequirement
    {
        private IConfiguration Configuration { get; }
        public string ClaimType;

        public BaseAuthorizationHandler(IConfiguration Configuration)
        {
            this.Configuration = Configuration;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, TRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == ClaimType))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            string value = ClaimType.IndexOf(".permission") > 0 ? ClaimType.Split('.')[0] : string.Empty;
            var userClaimValue = context.User.FindFirst(c => c.Type == ClaimType).Value;

            if (userClaimValue == value)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
