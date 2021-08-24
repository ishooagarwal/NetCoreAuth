using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetNull.Persistance.Models.ErrorResponses
{
    public class Errors : Response
    {
        public IList<Error> Error { get; set; }
    }
}
