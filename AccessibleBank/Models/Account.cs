//DataAnnotations provides attributes for model validation(e.g. [Required])
//DataAnnotations.Schema provides attributes for database schema mapping(e.g. [ForeignKey])
//Serialization provides attributes for JSON seralization control(e.g. [JsonIgnore])
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

//Namespace groups domain models under AccessibleBank.Models
namespace AccessibleBank.Models
{
    //AccountType enum, defines two possible account types
    public enum AccountType
    {
        Regular = 0,
        Savings = 1
    }

    //Account class represents a bank account entity in the system
    public class Account
    {
        //Id: Primary key for the account, automatically icremented by the database
        public int Id { get; set; }

        //UserId
        //Foreign key referencing the owning user
        //Marked as required to enforce that every account mst be tied to a user
        [Required]
        public int UserId { get; set; }

        //User navigation property
        //1 Prevents JSON serializers from including the full User object in API responses, avoiding circular references and reducing payload size
        //2 Explicity associates this navigation property with the UserId foreign key
        //3 User?: Nullable reference type, indicating the property may be null (e.g. when deserializing DTOs)
        [JsonIgnore]
        [ForeignKey("UserId")]
        public User? User { get; set; } //making the property nullable with ?

        //Decimal representing the account's current balance
        //Initialized to 0 by default for new accounts
        public decimal Balance { get; set; } = 0;

        //String ISO currency code for the account (e.g. "USD", "EUR", "AED")
        //Initiallized to "AED" by default, clients can override when creating a new account
        public string Currency { get; set; } = "AED";

        //Strongly typed AccountType enum indicating whether the account is Regular or Savings
        //Default value is Regular
        public AccountType Type { get; set; } = AccountType.Regular;
    }
}
