# Dhaka Trade Center Billing System
## Project Specification Document

### 1. Project Overview
#### 1.1 Purpose
To develop a Windows desktop application that streamlines the billing process for Dhaka Trade Center, enabling efficient generation and management of monthly utility bills for shop owners.

#### 1.2 Key Features
- Customer management
- Monthly bill generation
- Historical data tracking
- Multiple copy generation (Customer/Office)
- Automated calculations
- Report generation
- Data backup and recovery
- User authentication and authorization

### 2. Technical Specifications

#### 2.1 Technology Stack
- Framework: .NET 6.0 (WPF for modern UI)
- Database: SQL Server Express
- ORM: Entity Framework Core
- UI Components: DevExpress/Syncfusion WPF Controls
- Reporting: Crystal Reports/DevExpress Reporting
- Architecture: MVVM (Model-View-ViewModel)

#### 2.2 Database Schema

```sql
-- Core Tables
Customers (
    CustomerID INT PRIMARY KEY,
    Name VARCHAR(100),
    ShopNo VARCHAR(20),
    Floor VARCHAR(10),
    PhoneNumber VARCHAR(15),
    RegistrationDate DATE,
    Status BOOLEAN
)

BillingRates (
    RateID INT PRIMARY KEY,
    RateType VARCHAR(50),
    Rate DECIMAL(10,2),
    EffectiveDate DATE,
    EndDate DATE,
    IsActive BOOLEAN
)

MonthlyBills (
    BillID INT PRIMARY KEY,
    CustomerID INT,
    BillingMonth DATE,
    PresentReading DECIMAL(10,2),
    PreviousReading DECIMAL(10,2),
    ACPresentReading DECIMAL(10,2),
    ACPreviousReading DECIMAL(10,2),
    BlowerFanCharge DECIMAL(10,2),
    GeneratorCharge DECIMAL(10,2),
    ServiceCharge DECIMAL(10,2),
    DueDate DATE,
    Status VARCHAR(20),
    CreatedAt TIMESTAMP,
    LastModifiedAt TIMESTAMP
)

PaymentRecords (
    PaymentID INT PRIMARY KEY,
    BillID INT,
    AmountPaid DECIMAL(10,2),
    PaymentDate DATE,
    PaymentMethod VARCHAR(50),
    LatePaymentCharges DECIMAL(10,2),
    TransactionReference VARCHAR(50)
)
```

### 3. User Interface Design

#### 3.1 Core Modules

##### Dashboard
- Quick overview of pending bills
- Monthly collection statistics
- Due payment alerts
- Quick actions menu

##### Customer Management
- Add/Edit customer information
- View customer history
- Status management
- Search and filter capabilities

##### Billing Module
- Meter reading entry
- Automatic calculation
- Preview before generation
- Bulk bill generation
- Copy management (Customer/Office)

##### Reports Module
- Daily collection reports
- Monthly summaries
- Outstanding payment reports
- Customer statements
- Custom report generation

#### 3.2 Fixed vs Variable Components
##### Fixed Components
- Generator Charge
- Service Charge
- VAT Percentage
- Late Payment Percentage

##### Variable Components
- Electric Meter Reading
- AC Meter Reading
- Blower Fan Usage
- Due Date
- Payment Status

### 4. Business Logic Implementation

#### 4.1 Bill Calculation Formula
```csharp
public class BillCalculator
{
    public decimal CalculateBill(
        decimal presentReading,
        decimal previousReading,
        decimal acPresentReading,
        decimal acPreviousReading,
        decimal blowerCharge,
        BillingRates rates)
    {
        decimal regularUnits = presentReading - previousReading;
        decimal acUnits = acPresentReading - acPreviousReading;
        
        decimal regularCharge = regularUnits * rates.RegularRate;
        decimal acCharge = acUnits * rates.ACRate;
        
        decimal subtotal = regularCharge + acCharge + blowerCharge +
                          rates.GeneratorCharge + rates.ServiceCharge;
        
        return subtotal;
    }
}
```

### 5. Security Features
- Role-based access control
- Audit logging
- Data encryption
- Secure password storage
- Session management

### 6. Data Management
- Regular automated backups
- Data archival system
- Import/Export functionality
- Data validation rules
- Error logging and monitoring

### 7. Print Management
- Multiple copy templates
- Print preview
- Digital signature support
- Batch printing
- PDF generation and storage

### 8. Development Phases

#### Phase 1: Core Development (6 weeks)
- Database setup and basic CRUD
- User authentication
- Customer management
- Basic billing module

#### Phase 2: Advanced Features (4 weeks)
- Report generation
- Payment processing
- Historical data management
- Backup system

#### Phase 3: UI/UX Enhancement (3 weeks)
- Dashboard implementation
- Advanced search features
- Print template design
- User interface polish

#### Phase 4: Testing & Deployment (3 weeks)
- Unit testing
- Integration testing
- User acceptance testing
- Deployment preparation
- User training documentation

### 9. Quality Assurance
- Automated unit tests
- Integration tests
- User acceptance testing
- Performance testing
- Security audit

### 10. Maintenance Plan
- Regular backup verification
- Performance monitoring
- Security updates
- User support system
- Feature enhancement tracking

### 11. System Requirements
- Windows 10 or later
- 8GB RAM minimum
- 1GB free disk space
- 1920x1080 resolution minimum
- Network connectivity for multi-user setup