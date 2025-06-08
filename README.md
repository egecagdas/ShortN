# ShortN - URL Shortener

A simple URL shortener service built with ASP.NET Core and Entity Framework Core.

## Features
- Shorten URLs with optional custom codes
- Set TTL (time-to-live) for links
- Retrieve and delete shortened URLs
- Built-in validation using FluentValidation
- Secure secret management using .NET User Secrets

## Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- (Optional) SQLite or PostgreSQL (depending on your connection string)

## First-Time Setup

1. **Clone the repository:**
   ```powershell
   git clone https://github.com/egecagdas/ShortN
   cd ShortN_repo
   ```

2. **Restore dependencies:**
   ```powershell
   dotnet restore
   ```

3. **Set up the database connection string using User Secrets:**
   ```powershell
   cd ShortN
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your-connection-string>"
   ```
   - Example for SQLite: `"Data Source=shortn.db"`
   - Example for PostgreSQL: `"Server=localhost;Port=5432;Database=ShortN;User Id=postgres;Password=yourpassword;"`

4. **Apply database migrations:**
   ```powershell
   dotnet ef database update
   ```

5. **Run the application:**
   ```powershell
   dotnet run --project ShortN
   ```

   Make sure you are in the ShortN directory when running these commands.

6. **Open Swagger UI:**
   - Navigate to `https://localhost:5231/swagger` (or the URL shown in your terminal) to explore and test the API.

## Project Structure
- `ShortN/` - Main project directory
  - `Models/` - Data models
  - `Services/` - Business logic and services
  - `Routes/` - Minimal API route definitions
  - `Validators/` - FluentValidation validators
  - `Data/` - Database context
- `ShortN.Tests/` - Test project directory

## Notes
- **Secrets:** Never commit your real connection strings or secrets to source control. Use User Secrets for local development and environment variables or a secret manager for production.
- **Validation:** All incoming requests are validated using FluentValidation.
- **.gitignore:** The repository includes a .gitignore to exclude build artifacts, user secrets, and other sensitive files.

## Useful Commands
- List user secrets:
  ```powershell
  dotnet user-secrets list
  ```
- Set a user secret:
  ```powershell
  dotnet user-secrets set "Key" "Value"
  ```

## Running Tests
```powershell
dotnet test
```

---

Feel free to open issues or contribute! 