using AspNetNull.Persistance.Models.UserClaims;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace AspNetNull.WebAPI.Handlers.Requirements
{
    public class PermissionRequirements : IAuthorizationRequirement
    {
        public IList<Claims> Claims { get; }

        public PermissionRequirements(IList<Claims> Claims)
        {
            this.Claims = Claims;
        }
    }
}
