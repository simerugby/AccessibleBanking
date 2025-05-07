//EntityFrameworkCore for DbContext and Fluent API
//AccessibleBank.Models to reference the User, Account and Transaction model classes
using Microsoft.EntityFrameworkCore;
using AccessibleBank.Models;

//Namespace grouping data access classes under AccessibleBank.Data
namespace AccessibleBank.Data
{
    //DbContext subclass
    //BankingContext inherits from DbContext, the primary EF Core class for interacting with the database
    public class BankingContext : DbContext
    {
        //Constructor accepts DbContextOptions<BankingContext> and passes them to the base DbContext constructor,
        //allowing configuration(e.g. connection string) via dependecy injection
        public BankingContext(DbContextOptions<BankingContext> options) : base(options) {}

        //DbSets
        //1 Table of User entities
        //2 Table of Account entities
        //3 Table of Transaction entities
        //Each DbSet<T> allows querying and saving instances of that model
        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        //Fluent API configuration
        //Overrides OnModelCreating method to customize the EF Core model
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Calls base.OnModelCreating first to ensure default behavior
            base.OnModelCreating(modelBuilder);

            //Restrict cascade deletes for the Transaction entity
            //1 Configures the relationship to FromAccount(navigation property) and its foreign key FromAccountId
            //2 Uses .WithMany() to indicate no back-reference collection
            //3 Sets OnDelete(DeleteBehavior.Restrict) to prevent deleting an Account if transactions reference it
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.FromAccount)
                .WithMany()
                .HasForeignKey(t => t.FromAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            //Repeats configuration for ToAccount and ToAccountId to similarly restrict deletes
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.ToAccount)
                .WithMany()
                .HasForeignKey(t => t.ToAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            //Decimal precision configuration
            //For Account.Balance and Transaction.Amount properties(decimal types), sets the SQL precision to (18,2)
            //18 total digits, 2 decimal places
            //Ensures consistency and prevents EF Core warnings about default decimal mappings
            modelBuilder.Entity<Account>()
                .Property(a => a.Balance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasPrecision(18, 2);
        }
    }
}
