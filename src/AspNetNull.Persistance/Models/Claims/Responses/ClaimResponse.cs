using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetNull.Persistance.Models.Claims.Responses
{
    public class ClaimResponse : ModelResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
