//Authorization for [Authorize] attribute
//Mvc for controller base classes and attributes
//EntityFrameworkCore for async database operations
//AccessibleBank.Data for the BankingContext (EF DbContext)
//AccessibleBank.Models for the Account model
//Claims to extract user claims(e.g. user ID)
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AccessibleBank.Data;
using AccessibleBank.Models;
using System.Security.Claims;


//Namespace grouping all controllers under AccessibleBank.Controllers
namespace AccessibleBank.Controllers
{
    //Attributes on AccountsController, 1 enables API-specific conventions (model validation, automatic 400 responses, etc
    //2 prefix routes with api/accounts (controller name)
    //3 requires a valid JWT token for all actions
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    //Class inherits form ControllerBase, which provides the core functionality for ASP.NET Core MVC
    //minimal API controller functionality
    public class AccountsController : ControllerBase
    {
        //Dependency injection, 1 Declares a private _context field of type BankingContext
        //2 Constructor receives a BankingContext instance (EF DbContext) and assigns it
        private readonly BankingContext _context;
        public AccountsController(BankingContext context)
        {
            _context = context;
        }

        //CreateAccount endpoint
        //1 Responds to POST /api/accounts
        //2 Accepts an Account object in the request body(deserialized from JSON)
        //3 Returns an IActionResult asynchronously

        // POST: api/accounts
        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] Account account)
        {
            //Get the current user ID
            //1 Reads the NameIdentifier claim (typically the user ID stored in the JWT)
            //2 Parses it to an int
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            //Duplicate check
            //1 Queries the Accounts table to see if ana account already exists for this user with the same currency and type
            //2 If an account exists, returns a 400 BadRequest response with an error message 
            bool exists = await _context.Accounts.AnyAsync(a => a.UserId == userId && a.Currency == account.Currency && a.Type == account.Type);
            if (exists)
                return BadRequest("You already have this type of account in that currency");
            
            //Initialize fields
            //1 Sets UserId on the account entity to link it to the current user
            //2 Initializes Balance to zero for new accounts
            account.UserId = userId;
            account.Balance = 0;
            
            //Save new account
            //1 Adds the new account entity to the EF Accounts DbSet
            //2 Calls SaveChangesAsync to persist the changes to the database
            //3 Returns a HTTP 200 OK response with the created account object (included assigned Id)
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return Ok(account);
        }

        //GetMyAccounts endpoint
        //1 Responds to GET /api/accounts
        //2 Returns a list of the current user's accounts

        // GET: api/accounts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Account>>> GetMyAccounts()
        {
            //Fetch Accounts
            //1 Reads current user ID
            //2 Queries Accounts filtering by UserId
            //3 Converts to a List<Account> asynchronously
            //4 Returns HTTP 200 OK response with the array of accounts
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var accounts = await _context.Accounts
                .Where(a => a.UserId == userId)
                .ToListAsync();

            return Ok(accounts);
        }

        //GetAccount by ID endpoint
        //1 Responds to GET /api/accounts/{id} with a path parameter id
        //2 Returns a single account if owned by the current user

        // GET: api/accounts/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Account>> GetAccount(int id)
        {
            //Fetch Single Account
            //1 Reads current user ID
            //2 Queries for the first account matching both the requested id and the UserId
            //3 If not found, returns HTTP 404 NotFound with a message
            //4 Otherwise, returns HTTP 200 OK with the account
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (account == null)
                return NotFound("Account not found or you do not own it.");

            return Ok(account);
        }
    }
}
