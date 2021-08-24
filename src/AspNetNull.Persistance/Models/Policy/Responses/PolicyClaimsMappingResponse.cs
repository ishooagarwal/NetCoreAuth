using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AspNetNull.Persistance.Models.Policy
{
    public class PolicyClaimsMappingResponse : PolicyResponse
    {
        public IList<UserClaims.Claims> ClaimsIds { get; set; }
    }
}
