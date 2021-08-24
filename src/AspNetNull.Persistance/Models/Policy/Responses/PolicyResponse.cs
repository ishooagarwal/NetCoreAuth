using System.ComponentModel.DataAnnotations;

namespace AspNetNull.Persistance.Models.Policy
{
    public class PolicyResponse : ModelResponse
    {
        public string PolicyId { get; set; }

        public string Name { get; set; }
    }
}
