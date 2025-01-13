# Project Structure Documentation

## Solution Architecture

The solution follows a clean architecture pattern with these main projects:

### DTCBillingSystem.Core
Contains the domain models, interfaces, and business logic.

```
Core/
├── Models/              # Domain entities
├── Interfaces/          # Core interfaces
├── Services/            # Business logic services
└── Enums/              # Domain enumerations
```

### DTCBillingSystem.Infrastructure
Handles data access and external services.

```
Infrastructure/
├── Data/               # Database context and configurations
├── Repositories/       # Data access implementations
├── Services/          # External service implementations
└── Migrations/        # Database migrations
```

### DTCBillingSystem.UI
WPF user interface implementation.

```
UI/
├── ViewModels/        # MVVM ViewModels
├── Views/             # XAML Views
├── Services/          # UI-specific services
├── Controls/          # Custom controls
└── Resources/         # Styles and resources
```

## Key Components

### Models
- Customer
- BillingRate
- MonthlyBill
- PaymentRecord
- User
- AuditLog

### ViewModels
- DashboardViewModel
- CustomerManagementViewModel
- BillingViewModel
- ReportsViewModel
- SettingsViewModel

### Services
- BillCalculationService
- PrintService
- BackupService
- AuthenticationService
- ReportGenerationService

## Database Design
As per the specification document, using SQL Server Express with the following main tables:
- Customers
- BillingRates
- MonthlyBills
- PaymentRecords
- Users
- AuditLogs

## Development Guidelines

### Coding Standards
- Follow C# coding conventions
- Use async/await for I/O operations
- Implement proper exception handling
- Add XML documentation for public APIs

### MVVM Implementation
- Use commands for user actions
- Implement INotifyPropertyChanged
- Avoid code-behind
- Use dependency injection

### Security
- Implement role-based access control
- Use secure password storage
- Encrypt sensitive data
- Implement audit logging

### Testing
- Unit tests for business logic
- Integration tests for data access
- UI automation tests
- Mock external dependencies 