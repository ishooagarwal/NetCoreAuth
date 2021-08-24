using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetNull.Persistance.Models.Claims.Requests
{
    [Table("AspNetClaims")]
    [Index("Name", "Value", "ConcurrencyStamp", IsUnique = true)]
    public class ClaimRequest : Request
    {
        [Key]
        [Required]
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Value { get; set; }
    }
}
