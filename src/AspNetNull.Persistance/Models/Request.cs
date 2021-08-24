using System;
using System.ComponentModel.DataAnnotations;

namespace AspNetNull.Persistance.Models
{
    public class Request
    {
        [Required]
        [ConcurrencyCheck]
        public string ConcurrencyStamp { get; set; }

        [Required]
        public string CreatedBy { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool IsDisabled { get; set; }
    }
}
