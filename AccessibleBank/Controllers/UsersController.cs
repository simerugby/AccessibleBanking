//Authorization for the [Authorize] attribute
//Mvc for controller base and routing attributes
//EntityFrameworkCore for async database operations
//AccessibleBank.Data for the EF BankingContext
//AccessibleBank.Models for the User and other model classes
//JWT & Tokens for JWT generation
//Claims for creating JWT claims
//Text for encoding the JWT signing key
//AccessibleBank.DTOs for the LoginDto used in the login endpoint
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AccessibleBank.Data;
using AccessibleBank.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using AccessibleBank.DTOs;

//Controller definition
namespace AccessibleBank.Controllers
{
    //1 Applies API convenions and automatic model validation
    //2 Base route is api/users
    //3 Inherits form ControllerBase, providing minimal API functionality
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        //Dependecy Injection
        //1 _context: EF Core database context for accessing Users, Accounts, and Transactions
        //2 _config: application configuration for reading JWT settings (issuer, key, audience)
        private readonly BankingContext _context;
        private readonly IConfiguration _config;
        
        //3 Constructor assigns injected dependecies to private fields
        public UsersController(BankingContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        //Register endpoint
        //1 POST: api/users/register
        //2 Accepts a User object in the request body
        [HttpPost("register")]
        public async Task<IActionResult> Register(User user)
        {
            //Email uniqueness check
            //1 Queries the Users table for an existing email
            //2 Returns HTTP 400 Bad Request if the email is already registered
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                return BadRequest("Email already exists.");

            //Create user
            //1 Hashes the plain-text password using BCrypt
            //2 Adds the new User entiy to the context
            //3 Saves changes to persist the user
            //4 Returns HTTP 200 OK with a success message
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully");
        }

        //Login endpoint
        //1 POST: api/users/login
        //2 Accepts a LoginDto containing Email and Password
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginData)
        {
            //Credentials validation
            //1 Attempts to find a user by email
            //2 If not found or password hash does not verify, returns HTTP 401 Unauthorized
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginData.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginData.Password, user.Password))
                return Unauthorized("Invalid email or password.");

            //JWT issuance
            //1 Calls GenerateJwtToken to create a signed JWT for the authenticated user
            //2 Returns HTTP 200 OK with an object containing the token
            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        //JWT generation logic
        private string GenerateJwtToken(User user)
        {
            //1 Creates claims: NameIdentifier (userID) and Email
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email!)
            };

            //2 Reads the secret key from configuration and creates a SymmetricSecurityKey
            //3 Creates SigningCredentials using HMAC-SHA256
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //4 Constructs a JwtSecurityToken with issuer, audience, claims, expiry(2hours) and credential
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds);

            //5 Serializes the token to a string and returns it
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    
        //Delete user endpoint
        //1 DELETE: api/users
        //2 Secured by [Authorize], requiring a valid JWT
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> Delete()
        {
            //Identify and validate user
            //1 Extracts the authenticated user's ID from JWT claims
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            //2 Attempts to load the User entity, returns HTTP 404 NotFound if absent
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            // (optional)Cascade delete logic
            //1 Queries all Accounts owned by the user
            //2 Collects account IDs to filter related Transactions
            //3 Removes all matching Transaction entities
            //4 Removes all matching Account entities
            //5 Removes the User entity
            //6 Saves changes to perform deletions
            //7 Returns HTTP 200 OK with confirmation message
            var userAccounts = _context.Accounts.Where(a => a.UserId == userId);
            var userAccountIds = await userAccounts.Select(a => a.Id).ToListAsync();
            var userTransactions = _context.Transactions
                .Where(t => userAccountIds.Contains(t.FromAccountId) || userAccountIds.Contains(t.ToAccountId));

            _context.Transactions.RemoveRange(userTransactions);
            _context.Accounts.RemoveRange(userAccounts);
            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return Ok("User and all related data deleted.");
        }
    }
}
