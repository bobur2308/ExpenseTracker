# ExpenseTracker - Multi-tenant Expense Management System

A complete multi-tenant SaaS application for expense tracking built with .NET 8, PostgreSQL, and JWT authentication.

## 🎯 Features

- **Multi-tenancy**: Complete tenant isolation with subdomain-based access
- **Authentication & Authorization**: JWT-based authentication with role-based access control
- **Expense Management**: Create, update, delete, and track expenses
- **Category System**: Organize expenses with customizable categories
- **User Management**: Multiple users per tenant with different roles (Owner, Admin, Manager, User)
- **Subscription Plans**: Free, Basic, Pro, and Enterprise plans
- **RESTful API**: Clean API design with Swagger documentation

## 🏗️ Architecture

### Clean Architecture Layers
```
ExpenseTracker/
├── ExpenseTracker.Domain/          # Entities and business logic
├── ExpenseTracker.Application/     # Interfaces and services
├── ExpenseTracker.Infrastructure/  # Data access and external services
├── ExpenseTracker.API/            # Web API controllers
└── ExpenseTracker.Shared/         # DTOs and shared models
```

### Key Patterns Implemented

- **Repository Pattern**: Generic repository for data access
- **Dependency Injection**: Clean separation of concerns
- **Multi-tenancy**: Tenant context service with global query filters
- **JWT Authentication**: Secure token-based authentication

## 🛠️ Technology Stack

- **Backend**: .NET 8 (C#)
- **Database**: PostgreSQL 15
- **ORM**: Entity Framework Core 8
- **Authentication**: JWT Bearer tokens
- **API Documentation**: Swagger/OpenAPI
- **Password Hashing**: PBKDF2 with SHA512

## 📋 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 15+](https://www.postgresql.org/download/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [Postman](https://www.postman.com/) (optional, for testing)

## 🚀 Getting Started

### 1. Clone the Repository
```bash
git clone https://github.com/yourusername/ExpenseTracker.git
cd ExpenseTracker
```

### 2. Database Setup

Create PostgreSQL database and tables:
```sql
-- Connect to PostgreSQL
psql -U postgres

-- Create database
CREATE DATABASE expense_tracker;

-- Connect to the database
\c expense_tracker

-- Run the schema creation script (see Database Schema section below)
```

### 3. Configuration

Update `appsettings.json` in `ExpenseTracker.API` project:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=expense_tracker;Username=postgres;Password=your_password"
  },
  "JwtSettings": {
    "Secret": "your-super-secret-key-at-least-32-characters-long-for-security!",
    "Issuer": "ExpenseTracker",
    "Audience": "ExpenseTrackerUsers",
    "ExpiryInMinutes": 1440
  }
}
```

### 4. Build and Run
```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run the API
dotnet run --project ExpenseTracker.API
```

The API will be available at:
- Swagger UI: `http://localhost:5200/` or `https://localhost:7156/`
- API Base URL: `http://localhost:5200/api`

## 📊 Database Schema
```sql
-- Tenants table
CREATE TABLE tenants (
    id UUID PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    subdomain VARCHAR(100) NOT NULL UNIQUE,
    contact_email VARCHAR(256) NOT NULL,
    created_at TIMESTAMP NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT true,
    plan INTEGER NOT NULL DEFAULT 0,
    subscription_expires_at TIMESTAMP NULL,
    max_users INTEGER NOT NULL DEFAULT 5
);

-- Users table
CREATE TABLE users (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    email VARCHAR(256) NOT NULL,
    password_hash TEXT NOT NULL,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    role INTEGER NOT NULL DEFAULT 0,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP NOT NULL,
    last_login_at TIMESTAMP NULL,
    CONSTRAINT fk_users_tenants FOREIGN KEY (tenant_id) REFERENCES tenants(id) ON DELETE CASCADE
);

-- Categories table
CREATE TABLE categories (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    name VARCHAR(100) NOT NULL,
    description VARCHAR(500) NULL,
    color_code VARCHAR(7) NOT NULL DEFAULT '#000000',
    is_active BOOLEAN NOT NULL DEFAULT true,
    CONSTRAINT fk_categories_tenants FOREIGN KEY (tenant_id) REFERENCES tenants(id) ON DELETE CASCADE
);

-- Expenses table
CREATE TABLE expenses (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    user_id UUID NOT NULL,
    title VARCHAR(200) NOT NULL,
    description VARCHAR(1000) NULL,
    amount DECIMAL(18,2) NOT NULL,
    currency VARCHAR(3) NOT NULL DEFAULT 'USD',
    category_id UUID NOT NULL,
    expense_date TIMESTAMP NOT NULL,
    status INTEGER NOT NULL DEFAULT 0,
    receipt_url TEXT NULL,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NULL,
    CONSTRAINT fk_expenses_tenants FOREIGN KEY (tenant_id) REFERENCES tenants(id) ON DELETE CASCADE,
    CONSTRAINT fk_expenses_users FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE RESTRICT,
    CONSTRAINT fk_expenses_categories FOREIGN KEY (category_id) REFERENCES categories(id) ON DELETE RESTRICT
);

-- Indexes
CREATE UNIQUE INDEX idx_users_tenant_email ON users(tenant_id, email);
CREATE INDEX idx_categories_tenant_id ON categories(tenant_id);
CREATE UNIQUE INDEX idx_categories_tenant_name ON categories(tenant_id, name);
CREATE INDEX idx_expenses_tenant_id ON expenses(tenant_id);
CREATE INDEX idx_expenses_user_id ON expenses(user_id);
CREATE INDEX idx_expenses_category_id ON expenses(category_id);
CREATE INDEX idx_expenses_expense_date ON expenses(expense_date);
CREATE INDEX idx_expenses_status ON expenses(status);
```

## 🔑 API Endpoints

### Authentication

#### Register Tenant
```http
POST /api/Auth/RegisterTenant
Content-Type: application/json

{
  "companyName": "Test Company",
  "subdomain": "testcompany",
  "ownerEmail": "admin@test.com",
  "ownerFirstName": "John",
  "ownerLastName": "Doe",
  "password": "Test123!"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "admin@test.com",
    "firstName": "John",
    "lastName": "Doe",
    "role": "Owner"
  }
}
```

#### Login
```http
POST /api/Auth/Login
Content-Type: application/json

{
  "email": "admin@test.com",
  "password": "Test123!"
}
```

### Expenses (Requires Authentication)

#### Get All Expenses
```http
GET /api/Expenses
Authorization: Bearer {token}
```

#### Create Expense
```http
POST /api/Expenses
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "Office Supplies",
  "description": "Pens and notebooks",
  "amount": 50.00,
  "currency": "USD",
  "categoryId": "category-guid-here",
  "expenseDate": "2024-02-10T10:00:00Z"
}
```

#### Update Expense
```http
PUT /api/Expenses/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "Updated Title",
  "amount": 75.00
}
```

#### Delete Expense
```http
DELETE /api/Expenses/{id}
Authorization: Bearer {token}
```

#### Update Expense Status (Manager/Admin/Owner only)
```http
PATCH /api/Expenses/{id}/status
Authorization: Bearer {token}
Content-Type: application/json

1  // 0=Pending, 1=Approved, 2=Rejected, 3=Reimbursed
```

## 🎭 User Roles

- **Owner (3)**: Full access, cannot be deleted
- **Admin (2)**: Manage users and all expenses
- **Manager (1)**: Approve/reject expenses, view all expenses
- **User (0)**: Create and manage own expenses

## 💳 Subscription Plans

- **Free (0)**: Up to 5 users
- **Basic (1)**: Up to 20 users
- **Pro (2)**: Up to 100 users
- **Enterprise (3)**: Unlimited users

## 🧪 Testing

### Using Swagger

1. Navigate to `http://localhost:5200/`
2. Register a new tenant using `/api/Auth/RegisterTenant`
3. Copy the token from the response
4. Click "Authorize" button (top right)
5. Enter: `Bearer {your-token}`
6. Test other endpoints

### Using Postman

Import the API and test all endpoints with proper authentication headers.

## 📁 Project Structure
```
ExpenseTracker/
│
├── ExpenseTracker.API/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   └── ExpensesController.cs
│   ├── Middleware/
│   │   └── TenantResolverMiddleware.cs
│   ├── Program.cs
│   └── appsettings.json
│
├── ExpenseTracker.Application/
│   ├── Interfaces/
│   │   └── IRepository.cs
│   └── Services/
│       ├── ITenantService.cs
│       ├── ITokenService.cs
│       └── IPasswordHashService.cs
│
├── ExpenseTracker.Domain/
│   └── Entities/
│       ├── Tenant.cs
│       ├── User.cs
│       ├── Expense.cs
│       └── Category.cs
│
├── ExpenseTracker.Infrastructure/
│   ├── Data/
│   │   └── AppDbContext.cs
│   ├── Repositories/
│   │   └── Repository.cs
│   └── Services/
│       ├── TenantService.cs
│       ├── TokenService.cs
│       └── PasswordHashService.cs
│
└── ExpenseTracker.Shared/
    ├── DTOs/
    │   ├── LoginDto.cs
    │   ├── RegisterTenantDto.cs
    │   └── ExpenseDtos.cs
    └── Models/
        └── JwtSettings.cs
```

## 🔐 Security Features

- **Password Hashing**: PBKDF2 with 100,000 iterations and SHA512
- **JWT Tokens**: Secure token-based authentication with 24-hour expiry
- **Multi-tenancy Isolation**: Automatic tenant filtering at database level
- **Role-based Authorization**: Fine-grained access control
- **Query Filters**: Global filters prevent cross-tenant data access

## 🐛 Troubleshooting

### Database Connection Issues
```bash
# Test PostgreSQL connection
psql -U postgres -d expense_tracker

# Check if tables exist
\dt
```

### Build Errors
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

### Port Already in Use

Update `launchSettings.json` in `ExpenseTracker.API/Properties/` to use different ports.

## 📝 Future Enhancements

- [ ] File upload for receipt images
- [ ] Export expenses to CSV/Excel
- [ ] Dashboard with analytics
- [ ] Email notifications
- [ ] Audit logging
- [ ] Two-factor authentication
- [ ] Payment gateway integration
- [ ] Mobile app (React Native)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Author

**Bobur** - .NET Backend Developer

## 🙏 Acknowledgments

- Clean Architecture principles
- Multi-tenancy best practices
- JWT authentication patterns
- Entity Framework Core documentation

---

**Built with ❤️ using .NET 8**
