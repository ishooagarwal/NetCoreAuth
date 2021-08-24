using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetNull.Persistance.Models.Password
{
    public class ForgotPasswordRequest
    {
        [Required]
        public string Email { get; set; }
    }
}
