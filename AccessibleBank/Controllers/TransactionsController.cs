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


namespace AccessibleBank.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly BankingContext _context;

        public TransactionsController(BankingContext context)
        {
            _context = context;
        }

        // POST: api/transactions
        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] Transaction transaction)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var fromAccount = await _context.Accounts.FindAsync(transaction.FromAccountId);
            var toAccount = await _context.Accounts.FindAsync(transaction.ToAccountId);

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

            fromAccount.Balance -= transaction.Amount;
            toAccount.Balance += transaction.Amount;

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(transaction);
        }

        // GET: api/transactions
        [HttpGet("my")]
        public async Task<IActionResult> GetMyTransactions([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] decimal? minAmount = null, [FromQuery] decimal? maxAmount = null, [FromQuery] DateTime? dateFrom = null, [FromQuery] DateTime? dateTo = null, [FromQuery] string? category = null, [FromQuery] string? description = null)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var accountIds = await _context.Accounts
                .Where(a => a.UserId == userId)
                .Select(a => a.Id)
                .ToListAsync();
            
            var query = _context.Transactions
                .Where(t => accountIds.Contains(t.FromAccountId) || accountIds.Contains(t.ToAccountId));

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

            var transactions = await query
                .OrderByDescending(t => t.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(transactions);
        }

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

            // PDF generation
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
