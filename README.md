# TaskManager Pro

A full-stack web-based task management system built with **ASP.NET Core** and **React.js**. It enables teams to create, assign, and track tasks with role-based access control, real-time status updates, and a clean modern dashboard.

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | ASP.NET Core 10, Entity Framework Core, SQL Server |
| Frontend | React.js, TypeScript, Tailwind CSS |
| Auth | ASP.NET Identity, JWT Bearer Tokens |
| Logging | Serilog (Console + File) |
| Testing | xUnit, Moq, FluentAssertions |
| CI/CD | GitHub Actions |

---

## Prerequisites

Make sure you have the following installed before running the project:

- [.NET SDK 10.0+](https://dotnet.microsoft.com/download)
- [Node.js 18+ and npm](https://nodejs.org/)
- [SQL Server](https://www.microsoft.com/en-us/sql-server) or SQL Server Express
- [Git](https://git-scm.com/)

---

## Project Structure

```
TaskManagementTool/
├── Backend/
│   ├── TaskManagement.API/        # ASP.NET Core Web API
│   └── TaskManagement.Tests/      # xUnit Test Project
└── Frontend/
    └── task-ui/                   # React + TypeScript App
```

---

## Database Setup

### Step 1 — Find Your SQL Server Name

Open **SQL Server Management Studio (SSMS)** and check the server name in the login dialog. It usually looks like one of these:

```
localhost
localhost\SQLEXPRESS
DESKTOP-XXXXX\SQLEXPRESS
.\SQLEXPRESS
```

You can also run this query in SSMS to confirm:
```sql
SELECT @@SERVERNAME
```

### Step 2 — Verify Your Connection String

Open `Backend/TaskManagement.API/appsettings.json` and make sure the connection string matches your server name:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=YOUR_SERVER_NAME;Initial Catalog=TaskManagementDB;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;"
  }
}
```

### Step 3 — Database is Created Automatically

You do **not** need to create the database manually. When you run the backend for the first time, it will:

1. Automatically create the `TaskManagementDB` database
2. Create all tables (Users, Tasks, Categories, Roles)
3. Seed the default Admin user and roles

> ✅ Just make sure SQL Server is running before starting the backend.

### Step 4 — Verify Database Was Created

After running the backend, open **SSMS** and check:

```
Object Explorer → Databases → TaskManagementDB
```

You should see these tables:
```
TaskManagementDB/
├── Tables/
│   ├── AspNetUsers
│   ├── AspNetRoles
│   ├── AspNetUserRoles
│   ├── Tasks
│   └── Categories
```

### Troubleshooting Connection Issues

If the backend fails to connect, check these:

1. **SQL Server is running** — Open `Services` (Win+R → `services.msc`) → find `SQL Server` → make sure status is **Running**
2. **Server name is correct** — Open SSMS and confirm the exact server name
3. **Integrated Security** — Make sure your Windows user has access to SQL Server
4. **Firewall** — If using a remote server, make sure port `1433` is open

---

## Backend Setup

### 1. Clone the Repository

```bash
git clone https://github.com/hamzashakeel1221/TaskManagementTool.git
cd TaskManagementTool
```

### 2. Configure the Database

Open `Backend/TaskManagement.API/appsettings.json` and update the connection string with your SQL Server name (see **Database Setup** section above):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=YOUR_SERVER_NAME;Initial Catalog=TaskManagementDB;Integrated Security=True;Trust Server Certificate=True;"
  }
}
```

### 3. Configure JWT Secret

Create a file `Backend/TaskManagement.API/appsettings.Development.json` with your JWT secret:

```json
{
  "Jwt": {
    "Key": "YourSuperSecretKeyMinimum32CharsLong!!"
  }
}
```

> ⚠️ This file is gitignored and must be created manually on each machine.

### 4. NuGet Packages

All required packages are restored automatically via `dotnet restore`. Below is the full list of NuGet packages with exact versions.

#### TaskManagement.API

| Package | Version | Purpose |
|---------|---------|----------|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.8 | JWT authentication |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 10.0.8 | ASP.NET Identity with EF Core |
| `Microsoft.AspNetCore.OpenApi` | 10.0.8 | OpenAPI/Swagger support |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.8 | EF Core design-time tools |
| `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.8 | SQL Server database provider |
| `Microsoft.EntityFrameworkCore.Tools` | 10.0.8 | EF Core CLI tools (migrations) |
| `Serilog.AspNetCore` | 10.0.0 | Serilog integration for ASP.NET Core |
| `Serilog.Sinks.Console` | 6.1.1 | Log output to console |
| `Serilog.Sinks.File` | 7.0.0 | Log output to file |

#### TaskManagement.Tests

| Package | Version | Purpose |
|---------|---------|----------|
| `xunit` | 2.9.3 | Unit testing framework |
| `xunit.runner.visualstudio` | 3.1.4 | xUnit Visual Studio runner |
| `Microsoft.NET.Test.Sdk` | 17.14.1 | .NET test SDK |
| `Moq` | 4.20.72 | Mocking library |
| `FluentAssertions` | 6.12.2 | Readable test assertions |
| `Microsoft.EntityFrameworkCore.InMemory` | 10.0.8 | In-memory database for tests |
| `coverlet.collector` | 6.0.4 | Code coverage collector |

To install any missing package manually, run inside the project folder:

```bash
# API packages
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 10.0.8
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 10.0.8
dotnet add package Microsoft.AspNetCore.OpenApi --version 10.0.8
dotnet add package Microsoft.EntityFrameworkCore.Design --version 10.0.8
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 10.0.8
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 10.0.8
dotnet add package Serilog.AspNetCore --version 10.0.0
dotnet add package Serilog.Sinks.Console --version 6.1.1
dotnet add package Serilog.Sinks.File --version 7.0.0

# Test packages
dotnet add package xunit --version 2.9.3
dotnet add package xunit.runner.visualstudio --version 3.1.4
dotnet add package Microsoft.NET.Test.Sdk --version 17.14.1
dotnet add package Moq --version 4.20.72
dotnet add package FluentAssertions --version 6.12.2
dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 10.0.8
dotnet add package coverlet.collector --version 6.0.4
```

### 5. Restore Packages

```bash
cd Backend/TaskManagement.API
dotnet restore
```

### 6. Run Migrations

```bash
cd Backend/TaskManagement.API
dotnet ef database update
```

> If you do not have EF tools installed, run this first:
> ```bash
> dotnet tool install --global dotnet-ef
> ```

### 7. Run the API

```bash
dotnet run
```

The API will start at `http://localhost:5174` by default.

> The database is created automatically on first run using `EnsureCreatedAsync()`. Default admin user and roles are seeded automatically.

---

## Frontend Setup

### 1. Navigate to Frontend

```bash
cd Frontend/task-ui
```

### 2. Install Dependencies

```bash
npm install
```

### 3. Configure API Base URL

Create a `.env` file in `Frontend/task-ui/`:

```env
VITE_API_URL=http://localhost:5174/api
```

> Update the port if your backend runs on a different port.

### 4. Run the Frontend

```bash
npm run dev
```

The app will start at `http://localhost:5173`.

---

## How to Run the Full App

Follow these steps in order:

**Step 1 — Make sure SQL Server is running**

Open `Services` (Win+R → `services.msc`) → find `SQL Server` → confirm it is **Running**.

**Step 2 — Start the Backend**
```bash
cd Backend/TaskManagement.API
dotnet run
```

Wait until you see:
```
Now listening on: http://localhost:5174
```

**Step 3 — Start the Frontend**
```bash
cd Frontend/task-ui
npm run dev
```

**Step 4 — Open in Browser**

Navigate to: [http://localhost:5173](http://localhost:5173)

---

## Default Credentials

The following accounts are seeded automatically on first run:

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@taskmanager.com | Admin@123456 |

> Regular user accounts can be created via the **Register** page.

### Demo User Accounts

| Name | Email | Password |
|------|-------|----------|
| Haider Ali | haider1@gmail.com | Haider!123 |
| Jawad | Jawad1@gmail.com | Jawad!123 |
| Kamran | kamran1@gmail.com | Kamran!123 |
| Hamza | hamza1@gmail.com | Hamza!123 |
| Abdullah | abdullah1@gmail.com | Abdullah!123 |

---

## Running Tests

```bash
cd Backend/TaskManagement.Tests
dotnet test
```

Expected output:
```
Total: 58, Failed: 0, Succeeded: 58
```

---

## Role-Based Access

| Feature | Admin | Regular User | Assigned User |
|---------|-------|--------------|---------------|
| View all tasks | ✅ | ❌ (own only) | ✅ (assigned) |
| Create task | ✅ | ✅ | ✅ |
| Assign task to others | ✅ | ❌ | ❌ |
| Edit own task | ✅ | ✅ | ❌ |
| Edit others' tasks | ❌ | ❌ | ❌ |
| Update task status | ✅ | ✅ | ✅ |
| Delete task | ✅ (own only) | ✅ (own only) | ❌ |

---

## Application Screens

- **Login / Register** — JWT-based authentication
- **Dashboard** — Task counts by status (admin sees all, users see own)
- **Task List** — Filterable and searchable task list
- **Task Detail** — Full task info with edit/delete options
- **Create / Edit Task** — Form with role-based field access
- **Profile** — User info and logout

---

## Logging

Logs are stored in the `Backend/TaskManagement.API/logs/` directory with daily rotation:

```
logs/log-20260607.txt
logs/log-20260608.txt
```

---

## CI/CD Pipeline

GitHub Actions workflow runs on every push to `main`:

1. Build API
2. Build Tests
3. Run 58 unit tests with coverage
4. Send coverage report to SonarCloud

---