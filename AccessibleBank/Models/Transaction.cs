//DataAnnotations for validation attributes like [Required] or [MaxLength]
//Schema for database schema attributes like [ForeignKey]
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

//Namespace groups domain models under AccessibleBank.Models
namespace AccessibleBank.Models
{
    //Represents the financial transaction between two accounts
    public class Transaction
    {
        //Id: primary key for the transaction, auto-incremented by the database
        public int Id { get; set; }

        //Foreign key referencing the source Account
        //Required ensures it must be provided
        [Required]
        public int FromAccountId { get; set; }

        //Foreign key referencing the destination Account
        //Also required
        [Required]
        public int ToAccountId { get; set; }

        //Decimal value representing the transaction amount
        [Required]
        public decimal Amount { get; set; }

        //DateTime of when the transaction was created
        //Defaults to DateTime.UtcNow if not provided, capturing creation timestamp in UTC
        public DateTime Date { get; set; } = DateTime.UtcNow;
        
        //Optional text giving details about the transaction, limit to 255 chararacters
        [MaxLength(255)]
        public string? Description { get; set; }

        //Optional short label categorizing the transaction, limit to 50 char
        [MaxLength(50)]
        public string? Category { get; set; }

        //References the Account entity corresponding to FromAccountId
        //[ForeignKey("FromAccountId")] links it to the foreign key property
        //Nullable, because EF may not load it if not explicitly included
        [ForeignKey("FromAccountId")]
        public Account? FromAccount { get; set; }

        //Same as FromAccount
        [ForeignKey("ToAccountId")]
        public Account? ToAccount { get; set; }
    }
}
