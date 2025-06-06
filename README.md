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
   ```sh
   git clone <your-repo-url>
   cd abat_assignment
   ```

2. **Restore dependencies:**
   ```sh
   dotnet restore
   ```

3. **Set up the database connection string using User Secrets:**
   ```sh
   cd ShortN
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your-connection-string>"
   ```
   - Example for SQLite: `"Data Source=shortn.db"`
   - Example for PostgreSQL: `"Server=localhost;Port=5432;Database=ShortN;User Id=postgres;Password=yourpassword;"`

4. **Apply database migrations (if any):**
   ```sh
   dotnet ef database update
   ```

5. **Run the application:**
   ```sh
   dotnet run --project ShortN
   ```

6. **Open Swagger UI:**
   - Navigate to `https://localhost:5001/swagger` (or the URL shown in your terminal) to explore and test the API.

## Project Structure
- `ShortN/Models/` - Data models
- `ShortN/Services/` - Business logic and services
- `ShortN/Routes/` - Minimal API route definitions
- `ShortN/Validators/` - FluentValidation validators
- `ShortN/Data/` - Database context

## Notes
- **Secrets:** Never commit your real connection strings or secrets to source control. Use User Secrets for local development and environment variables or a secret manager for production.
- **Validation:** All incoming requests are validated using FluentValidation.
- **.gitignore:** The repository includes a .gitignore to exclude build artifacts, user secrets, and other sensitive files.

## Useful Commands
- List user secrets:
  ```sh
  dotnet user-secrets list
  ```
- Set a user secret:
  ```sh
  dotnet user-secrets set "Key" "Value"
  ```

---

Feel free to open issues or contribute! 