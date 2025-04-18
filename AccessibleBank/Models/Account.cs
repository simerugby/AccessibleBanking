using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


namespace AccessibleBank.Models
{
        public enum AccountType
    {
        Regular = 0,
        Savings = 1
    }

    public class Account
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [JsonIgnore]
        [ForeignKey("UserId")]
        public User? User { get; set; } //making the property nullable with ?

        public decimal Balance { get; set; } = 0;

        public string Currency { get; set; } = "AED";

        public AccountType Type { get; set; } = AccountType.Regular;
    }
}
