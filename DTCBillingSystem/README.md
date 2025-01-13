# Dhaka Trade Center Billing System

A modern Windows desktop application for managing billing operations at Dhaka Trade Center.

## Prerequisites

- Windows 10 or later
- .NET 6.0 SDK
- SQL Server Express 2019 or later
- Visual Studio 2022 (recommended) or VS Code with C# extensions

## Installation

1. Install .NET 6.0 SDK from [https://dotnet.microsoft.com/download/dotnet/6.0](https://dotnet.microsoft.com/download/dotnet/6.0)
2. Install SQL Server Express from [https://www.microsoft.com/en-us/sql-server/sql-server-downloads](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
3. Clone this repository
4. Open the solution in Visual Studio 2022
5. Restore NuGet packages
6. Update the connection string in `appsettings.json`
7. Run database migrations
8. Build and run the application

## Features

- Customer Management
- Monthly Bill Generation
- Historical Data Tracking
- Multiple Copy Generation (Customer/Office)
- Automated Calculations
- Report Generation
- Data Backup and Recovery
- User Authentication and Authorization

## Project Structure

```
DTCBillingSystem/
├── src/                    # Source code
├── docs/                   # Documentation
└── tests/                  # Test projects
```

## Technology Stack

- Framework: .NET 6.0 (WPF)
- Database: SQL Server Express
- ORM: Entity Framework Core
- UI Components: DevExpress/Syncfusion WPF Controls
- Reporting: Crystal Reports/DevExpress Reporting
- Architecture: MVVM (Model-View-ViewModel)

## License

Proprietary - All rights reserved

## Support

For support, please contact the development team. 