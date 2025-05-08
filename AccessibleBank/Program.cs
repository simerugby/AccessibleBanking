//AccessibleBank.Data to register the BankingContext for EF Core
//EntityFrameworkCore for confuguring the SQL Server DbContext
//JwtBearer and Tokens to set up JWT bearer authentication
//Text for encoding the JWT signing key
using AccessibleBank.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

//Creates a new WebApplicationBuilder using provided command-line args. This initializes
// configuration, logging and depedency injection
var builder = WebApplication.CreateBuilder(args);

//Defines a constant name for the CORS policy that will be configured later
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

//Configures QuestPDF(used for PDF exports) to use the community license
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

//CORS configuration
//Registers CORS services and defines a policy named _myAllowSpecificOrigins
//Allows requests from the frontend origin :5173 (Vite dev server)
//Permits any header and HTTP method for that origin
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")  // frontend port
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

//Database context registration:
//Adds BankingContext as a service, configuring it to use SQL Server with the given connection string
//Connection string points to local SQL Server instance localhost\ACCESSIBLEBANK and database AccessibleBankDB with credentials
builder.Services.AddDbContext<BankingContext>(options =>
    options.UseSqlServer("Server=localhost\\ACCESSIBLEBANK;Database=AccessibleBankDB;User Id=sa;Password=nextstepdubai;TrustServerCertificate=True;"));

//Swagger/OpenAPI setup:
//1 AddEndpointsApiExplorer(): exposes minimal API endpoints for Swagger
//2 AddControllers(): registers MVC controllers
//3 AddSwaggerGen(): configures Swagger generation:
//3.1 Defines a document named v1
//3.2 Adds a security definition for Bearer JWT in the Authorization header
//3.3 Applies a global security requirement so endpoints require the Bearer scheme in Swagger UI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "AccessibleBank API", Version = "v1" });

    
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

//Authentication & Authorization:
//1 Registers JWT bearer authentication with default scheme
//2 Configures token validation parameters using settings from appsettings.json: Validates issuer, audience, token expiry, and signing key
//3 Registers authorization services
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

//Builds the WebApplication instance from the configured builder (finalizes service registrations and middleware configuration)
var app = builder.Build();

//Development-only middleware: If running in the Development environment, enables Swagger middleware and UI at /swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Global middleware order:
//1 UseHttpsRedirection(): redirects HTTP requests to HTTPS
//2 UseAuthentication(): enables the authentication middleware to validate JWT tokens
//3 UseCors(...): applies the CORS policy to incoming requests
//4 UseAuthorization(): enforces authorization policies
//5 MapControllers(): maps attribute-routed controllers to endpoints
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseCors(MyAllowSpecificOrigins);
app.UseAuthorization();
app.MapControllers();

//Starts the application, listening for incoming HTTP requests
app.Run();
