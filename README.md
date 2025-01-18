# DTC Billing System

A comprehensive billing system for managing customer accounts, meter readings, and payment processing.

## Project Structure

- **DTCBillingSystem.Core**: Contains business logic, services, and domain models
- **DTCBillingSystem.Infrastructure**: Data access layer and external service implementations
- **DTCBillingSystem.Shared**: Shared models, interfaces, and utilities
- **DTCBillingSystem.UI**: User interface project

## Prerequisites

- .NET 6.0 SDK or later
- Windows PowerShell
- Visual Studio 2022 (optional)

## Building and Running

### Using PowerShell Script

1. Open PowerShell
2. Navigate to the solution directory
3. Run the build script:
```powershell
.\build.ps1
```

### Manual Build

1. Open PowerShell
2. Navigate to the solution directory
3. Run the following commands:
```powershell
dotnet restore
dotnet build
dotnet run --project DTCBillingSystem.UI/DTCBillingSystem.UI.csproj
```

## Features

- Customer Management
- Meter Reading Management
- Billing Generation
- Payment Processing
- Audit Logging
- Notifications
- Report Generation and Printing

## Architecture

The application follows a clean architecture pattern with the following layers:
- Core (Business Logic)
- Infrastructure (Data Access)
- Shared (Common Components)
- UI (Presentation) 

