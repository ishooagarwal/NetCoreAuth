using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetNull.Persistance.Models.LockOut
{
    public class LockUserRequest
    {
        [Required]
        public string UserId { get; set; }
        public DateTimeOffset LockOutEndDate { get; set; }
    }
}
