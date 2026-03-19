<div align="center">

# DTC Billing System

A comprehensive Windows desktop billing application for Dhaka Trade Center, built to streamline monthly utility bill generation, customer management, payment tracking, and financial reporting for commercial shop owners.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-12-239120?style=for-the-badge&logo=csharp&logoColor=white)
![WPF](https://img.shields.io/badge/WPF-Desktop-0078D4?style=for-the-badge&logo=windows&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-Express-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)
![EF Core](https://img.shields.io/badge/EF_Core-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)

</div>

---

## Features

- **Dashboard** — Overview of pending bills, monthly collection statistics, due payment alerts, and quick actions
- **Customer Management** — Add, edit, search, and filter shop owner records with full status tracking
- **Monthly Bill Generation** — Automated utility bill calculation based on meter readings (electricity, AC, blower fan, generator, service charges)
- **Payment Tracking** — Record payments with multiple payment methods, late fee calculation, and transaction references
- **Bill Preview & Printing** — Preview bills before generation with dual-copy support (Customer / Office)
- **Billing Rate Management** — Configure and manage utility rates with effective date ranges
- **Audit Logging** — Complete trail of all system actions for accountability and compliance
- **Data Backup & Recovery** — Built-in backup and restore functionality for database safety
- **Notification System** — Automated alerts for due payments and system events
- **Report Generation** — Financial and operational reports for management review
- **User Authentication** — Secure login with role-based access control
- **Material Design UI** — Modern, clean interface using Material Design themes

## Architecture

The application follows **Clean Architecture** with the **MVVM** (Model-View-ViewModel) pattern:

```
DTCBillingSystem/
├── DTCBillingSystem.Core/            # Domain layer
│   ├── Models/                       # Entity models
│   │   ├── Customer.cs
│   │   ├── MonthlyBill.cs
│   │   ├── PaymentRecord.cs
│   │   ├── BillingRate.cs
│   │   ├── AuditLog.cs
│   │   └── User.cs
│   ├── Interfaces/                   # Service contracts
│   │   ├── IBillingService.cs
│   │   ├── IAuditService.cs
│   │   ├── IBackupService.cs
│   │   ├── INotificationService.cs
│   │   ├── IPrintService.cs
│   │   ├── IReportService.cs
│   │   ├── IUserService.cs
│   │   └── IRepository.cs
│   └── Services/                     # Business logic implementations
│
├── DTCBillingSystem.Infrastructure/  # Data access layer
│   └── Data/
│       ├── ApplicationDbContext.cs    # EF Core DbContext
│       └── Configurations/           # Fluent API entity configurations
│
└── DTCBillingSystem.UI/             # Presentation layer (WPF)
    ├── Views/                        # XAML views
    │   ├── DashboardView.xaml
    │   ├── CustomerView.xaml
    │   ├── CustomerBillsView.xaml
    │   ├── CustomerDialog.xaml
    │   ├── BillGenerationDialog.xaml
    │   ├── LoginWindow.xaml
    │   └── MainWindow.xaml
    ├── ViewModels/                   # MVVM ViewModels
    ├── Commands/                     # ICommand implementations
    └── Converters/                   # Value converters
```

## Tech Stack

| Layer | Technology |
|---|---|
| **Framework** | .NET 8.0, WPF |
| **Language** | C# 12 |
| **Database** | SQL Server Express |
| **ORM** | Entity Framework Core 8 |
| **UI Theme** | Material Design (MaterialDesignThemes, MaterialDesignColors) |
| **MVVM Toolkit** | CommunityToolkit.Mvvm |
| **DI** | Microsoft.Extensions.DependencyInjection |
| **Serialization** | Newtonsoft.Json |

## Getting Started

### Prerequisites

- **Visual Studio 2022** (v17.8+) with the **.NET Desktop Development** workload
- **.NET 8.0 SDK**
- **SQL Server Express** (LocalDB or full instance)

### Installation

1. **Clone the repository**

   ```bash
   git clone https://github.com/mhmalvi/DTC.git
   cd DTC/DTCBillingSystem
   ```

2. **Open the solution**

   Open `DTCBillingSystem.sln` in Visual Studio 2022.

3. **Configure the database connection**

   Update the connection string in `DTCBillingSystem.UI/appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\mssqllocaldb;Database=DTCBillingSystem;Trusted_Connection=True;"
     }
   }
   ```

4. **Apply database migrations**

   Open the Package Manager Console in Visual Studio and run:

   ```powershell
   Update-Database -Project DTCBillingSystem.Infrastructure
   ```

5. **Build and run**

   Press `F5` or click **Start** in Visual Studio. The application will launch with the login window.

### Build from Command Line

```bash
dotnet restore
dotnet build
dotnet run --project DTCBillingSystem.UI
```

## License

This project is open source and available under the [MIT License](LICENSE).
