//Authorization for [Authorize] to secure endpoints
//Mvc for defining controller actions and routing
//EntityFrameworkCore for async database operations
//AccessibleBank.Data for BankingContext (EF DbContext)
//AccessibleBank.Models for the Transaction model
//Claims to extract user identity from JWT
//System.Text for StringBuilder in CSV export
//QuestPDF.* for PDF document generation

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AccessibleBank.Data;
using AccessibleBank.Models;
using System.Security.Claims;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;

//Namespace grouping all controllers
namespace AccessibleBank.Controllers
{
    //Controller definition
    //1 API convertions
    //2 routes prefixed eith api/transactions
    //3 requires valid JWT for all actions
    //4 Inherits ControllerBase
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        //Dependency injection
        //_context: EF Core BankingContext for data access
        //Constructor injects the context
        private readonly BankingContext _context;

        public TransactionsController(BankingContext context)
        {
            _context = context;
        }

        //CreateTransaction endpoint
        //1 POST: api/transactions
        //2 Accepts a Transaction object in the request body
        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] Transaction transaction)
        {
            //Flow
            //1 Extracts current user's ID from JWT claims
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            //2 Loads source (fromAccount) and destination (toAccount) by their IDs
            var fromAccount = await _context.Accounts.FindAsync(transaction.FromAccountId);
            var toAccount = await _context.Accounts.FindAsync(transaction.ToAccountId);

            //3 Validates: both accounts exist, user owns the source account, sufficient funds exist,
            //accounts are distinct and both accounts use the same currency
            if (fromAccount == null || toAccount == null)
                return NotFound("One or both accounts not found.");

            if (fromAccount.UserId != userId)
                return Unauthorized("You don't own the source account.");

            if (fromAccount.Balance == 0)
                return BadRequest("Insufficient funds.");

            if (fromAccount.Balance < transaction.Amount)
                return BadRequest("Insufficient funds.");

            if (transaction.FromAccountId == transaction.ToAccountId)
                return BadRequest("You cannot transfer to the same account.");

            if (fromAccount.Currency != toAccount.Currency)
                return BadRequest("Cannot transfer between accounts with different currencies.");

            //Adjusts balances
            fromAccount.Balance -= transaction.Amount;
            toAccount.Balance += transaction.Amount;

            //Adds the transaction entity to the context and saves changes
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            //Returns HTTP 200 OK with the created transaction
            return Ok(transaction);
        }

        //GetMyTransactions endpoint
        // GET: api/transactions/my
        //Supports pagination(page, pageSize) and optional filters
        [HttpGet("my")]
        public async Task<IActionResult> GetMyTransactions([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] decimal? minAmount = null, [FromQuery] decimal? maxAmount = null, [FromQuery] DateTime? dateFrom = null, [FromQuery] DateTime? dateTo = null, [FromQuery] string? category = null, [FromQuery] string? description = null)
        {
            //Build base query
            //1 Retrieves all account IDs belonging to the user
            //2 Filters transactions where either source or destination account is owned by the user
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var accountIds = await _context.Accounts
                .Where(a => a.UserId == userId)
                .Select(a => a.Id)
                .ToListAsync();
            var query = _context.Transactions
                .Where(t => accountIds.Contains(t.FromAccountId) || accountIds.Contains(t.ToAccountId));

            //Apply filters only if parameters are provided
            if (minAmount.HasValue)
                query = query.Where(t => t.Amount >= minAmount.Value);

            if (maxAmount.HasValue)
                query = query.Where(t => t.Amount <= maxAmount.Value);

            if (dateFrom.HasValue)
                query = query.Where(t => t.Date >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(t => t.Date <= dateTo.Value);

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(t => t.Category != null && t.Category.Contains(category));

            if (!string.IsNullOrWhiteSpace(description))
                query = query.Where(t => t.Description != null && t.Description.Contains(description));

            //Pagination and execution
            //1 Sorts by date descending
            //2 Skips and takes based on page and pageSize
            //3 Executes the query and returns the result list
            var transactions = await query
                .OrderByDescending(t => t.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(transactions);
        }

        //Export endpoint setup
        //1 Handles GET /api/transactions/export with same filters + format
        //2 Validates if format is csv or pdf
        //3 Reuses account ID retrieval and filters to build transactions list
        //4 Also loads a dictionary of account IDs to their currency codes for export display
        [HttpGet("export")]
        public async Task<IActionResult> Export([FromQuery] string format = "csv", [FromQuery] string? category = null, [FromQuery] DateTime? dateFrom = null, [FromQuery] DateTime? dateTo = null, [FromQuery] decimal? minAmount = null, [FromQuery] decimal? maxAmount = null)
        {
            format = format.ToLower();

            if (format != "csv" && format != "pdf")
                return BadRequest("Invalid format. Use 'csv' or 'pdf'.");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var accountIds = await _context.Accounts
                .Where(a => a.UserId == userId)
                .Select(a => a.Id)
                .ToListAsync();

            var transactionsQuery = _context.Transactions
                .Where(t => accountIds.Contains(t.FromAccountId) || accountIds.Contains(t.ToAccountId));

            if (!string.IsNullOrWhiteSpace(category))
                transactionsQuery = transactionsQuery.Where(t => t.Category != null && t.Category.Contains(category));

            if (dateFrom.HasValue)
                transactionsQuery = transactionsQuery.Where(t => t.Date >= dateFrom.Value);

            if (dateTo.HasValue)
                transactionsQuery = transactionsQuery.Where(t => t.Date <= dateTo.Value);

            if (minAmount.HasValue)
                transactionsQuery = transactionsQuery.Where(t => t.Amount >= minAmount.Value);

            if (maxAmount.HasValue)
                transactionsQuery = transactionsQuery.Where(t => t.Amount <= maxAmount.Value);

            var transactions = await transactionsQuery
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            var accountCurrencies = await _context.Accounts
                .Where(a => accountIds.Contains(a.Id))
                .ToDictionaryAsync(a => a.Id, a => a.Currency);

            //CSV generation
            //1 Builds header row and data rows using StringBuilder(using european separation ";")
            //2 Looks up currency for each transaction's source account
            //3 Returns a file result with MIME type text/csv
            if (format == "csv")
            {
                var csv = new StringBuilder();
                csv.AppendLine("Id;FromAccountId;ToAccountId;Amount;Currency;Date;Description;Category");

                foreach (var t in transactions)
                {
                    var currency = (await _context.Accounts.FindAsync(t.FromAccountId))?.Currency ?? "N/A";
                    csv.AppendLine($"{t.Id};{t.FromAccountId};{t.ToAccountId};{t.Amount};{currency};{t.Date:yyyy-MM-dd HH:mm:ss};{t.Description};{t.Category}");
                }

                var bytes = Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", "transactions.csv");
            }

            //PDF generation
            //1 Creates a document
            //2 Adds a centered, bold header
            //3 Builds a table with 4 relative-width columns
            //4 Renders a header row with bold column titles
            //5 Iterates over transactions, adding rows with cells (CellStyle applies padding and bottom border)
            //6 Generates the PDF to a byte array and returns it as application/pdf
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.Header().Text("Transaction Report").FontSize(20).Bold().AlignCenter();

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        // Header row
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("From → To").Bold();
                            header.Cell().Element(CellStyle).Text("Amount & Currency").Bold();
                            header.Cell().Element(CellStyle).Text("Date").Bold();
                            header.Cell().Element(CellStyle).Text("Category / Description").Bold();
                        });

                        foreach (var t in transactions)
                        {
                            var currency = accountCurrencies.TryGetValue(t.FromAccountId, out var c) ? c : "N/A";

                            table.Cell().Element(CellStyle).Text($"{t.FromAccountId} → {t.ToAccountId}");
                            table.Cell().Element(CellStyle).Text($"{t.Amount:0.00} {currency}");
                            table.Cell().Element(CellStyle).Text(t.Date.ToString("yyyy-MM-dd"));
                            table.Cell().Element(CellStyle).Text($"{t.Category} / {t.Description}");
                        }

                        IContainer CellStyle(IContainer container) =>
                            container.Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                    });
                });
            });

            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", "transactions.pdf");
        }
    }
}
