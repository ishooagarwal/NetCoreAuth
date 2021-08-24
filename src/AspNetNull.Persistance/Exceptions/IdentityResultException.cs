using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetNull.Persistance.Exceptions
{
    public class IdentityResultException : Exception
    {
        public IdentityResultException(string message) : base(message)
        {

        }
    }
}
