using AspNetNull.Persistance.Models.Application;
using System.Collections.Generic;

namespace AspNetNull.Persistance.Models.Roles
{
    public class RetrieveRolesResponse : Response
    {
        public IList<ApplicationRole> roles { get; set; }
    }
}
