# AccessibleBank

## Project Overview

AccessibleBank is a full-stack banking application designed with accessibility and simplicity in mind. It provides users with the ability to register, authenticate, manage multiple accounts in different currencies, perform transfers between accounts, and export transaction history in CSV or PDF formats.

## Technologies

* **Backend**: ASP.NET Core 7, Entity Framework Core, SQL Server, JWT Authentication, QuestPDF
* **Frontend**: React (Vite), React Router, Context API, Tailwind CSS
* **Authentication**: JSON Web Tokens (JWT) with secure password hashing via BCrypt
* **PDF Generation**: QuestPDF

## Features

* **User Management**: Register, login, secure password storage, delete user and related data
* **Account Management**: Create and list user accounts by currency and type (Regular, Savings)
* **Transactions**: Create transfers between accounts, with balance checks and currency validation
* **Export**: Download transaction history as CSV or PDF report
* **Filtering & Pagination**: Query transactions by amount range, date range, category, description, with paging support

## Prerequisites

* [.NET SDK 7.0+](https://dotnet.microsoft.com/download)
* [Node.js v16+](https://nodejs.org/) and npm
* SQL Server instance (or Docker container)

## Repository Structure

```
backend/                # ASP.NET Core Web API
  Controllers/          # API controllers for Users, Accounts, Transactions
  Data/                 # EF Core DbContext and configurations
  Models/               # Entity models: User, Account, Transaction
  DTOs/                 # Data transfer objects (LoginDto)
  Program.cs            # Application bootstrap and middleware
  appsettings.json      # Connection strings and JWT settings
frontend/               # React application
  src/
    components/         # Reusable UI components
    pages/              # Route components (LoginPage, Dashboard)
    context/            # AuthContext for global auth state
    App.jsx             # Router setup
  tailwind.config.js    # Tailwind CSS configuration
  postcss.config.js     # PostCSS setup
```

## Setup & Installation

### Backend

1. **Configure Database**

   * Update the connection string in `appsettings.json` under `ConnectionStrings:DefaultConnection`.
2. **Run Migrations**

   ```bash
   cd backend
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```
3. **Run API**

   ```bash
   dotnet run
   ```

   The API will listen on `https://localhost:5129` by default.

### Frontend

1. **Install Dependencies**

   ```bash
   cd frontend
   npm install
   ```
2. **Configure API Origin**

   * Ensure CORS in the API allows `http://localhost:5173` (configured in `Program.cs`).
3. **Run Development Server**

   ```bash
   npm run dev
   ```

   The SPA will be accessible at `http://localhost:5173`.

## Environment Variables & Configuration

* **appsettings.json**

  * `ConnectionStrings:DefaultConnection`: Connection string to your SQL Server database
  * `Jwt:Key`: Secret key for signing tokens
  * `Jwt:Issuer`: Token issuer identifier
  * `Jwt:Audience`: Allowed audience for tokens

* **Local Storage** (frontend)

  * JWT token is stored under `localStorage.token` after login

## Authentication Flow

1. **Register**: `POST /api/users/register` with `{ name, email, password }`
2. **Login**: `POST /api/users/login` with `{ email, password }` → returns `{ token }`
3. **Protected Endpoints**: Include header `Authorization: Bearer {token}`

## API Endpoints

### Users

* `POST /api/users/register`
* `POST /api/users/login`
* `DELETE /api/users` (requires auth)

### Accounts (require auth)

* `POST /api/accounts` → create new account
* `GET /api/accounts` → list user accounts
* `GET /api/accounts/{id}` → retrieve specific account

### Transactions (require auth)

* `POST /api/transactions` → create transfer
* `GET /api/transactions/my` → list transactions with filters and pagination
* `GET /api/transactions/export?format=csv|pdf&...` → download CSV or PDF

## Frontend Usage

* **Login**: Navigate to `/`, submit credentials
* **Dashboard**: `/dashboard` (protected)

  * Create new accounts
  * View balances
  * Initiate transfers via form
  * Filter & export transactions

## Contributing

Contributions, issues, and feature requests are welcome! Please open an issue or submit a pull request.

## License

This project is licensed under the MIT License.
