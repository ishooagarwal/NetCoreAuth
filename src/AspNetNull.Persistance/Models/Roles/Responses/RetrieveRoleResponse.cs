using AspNetNull.Persistance.Models.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetNull.Persistance.Models.Roles
{
    public class RetrieveRoleResponse : Response
    {
        public ApplicationRole role { get; set; }
    }
}
