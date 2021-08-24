using System.ComponentModel.DataAnnotations;

namespace AspNetNull.Persistance.Models.Policy.Requests
{
    public class PolicyRequest : Request
    {
        [Key]
        [Required]
        public string PolicyId { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
