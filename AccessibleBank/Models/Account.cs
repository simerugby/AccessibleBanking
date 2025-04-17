using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


namespace AccessibleBank.Models
{
    public class Account
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [JsonIgnore]
        [ForeignKey("UserId")]
        public User? User { get; set; } //making the property nullable with ?

        public decimal Balance { get; set; }

        public string Currency { get; set; } = "AED";
    }
}
