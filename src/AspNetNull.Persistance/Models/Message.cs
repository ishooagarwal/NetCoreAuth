using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetNull.Persistance.Models.Email
{
    public class Message
    {
        [Required]
        public string Subject { get; set; }
        [Required]
        public string Body { get; set; }
        [Required]
        public string Destination { get; set; }
        [Required]
        public string SenderEmail { get; set; }
    }
}
