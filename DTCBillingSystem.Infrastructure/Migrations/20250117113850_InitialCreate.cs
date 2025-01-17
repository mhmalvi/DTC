using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DTCBillingSystem.Infrastructure.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BillingRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CustomerType = table.Column<int>(type: "INTEGER", nullable: false),
                    BaseRate = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ExcessRate = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Threshold = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    FixedCharges = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingRates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountNumber = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "TEXT", nullable: false),
                    ContactNumber = table.Column<string>(type: "TEXT", nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    CustomerType = table.Column<int>(type: "INTEGER", nullable: false),
                    ZoneCode = table.Column<string>(type: "TEXT", nullable: false),
                    MeterNumber = table.Column<string>(type: "TEXT", nullable: false),
                    ShopNo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Floor = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ConnectionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false),
                    SecurityDeposit = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    CurrentBalance = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    LastBillingDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastPaymentDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<byte[]>(type: "BLOB", nullable: false),
                    PasswordSalt = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Role = table.Column<int>(type: "INTEGER", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    RequirePasswordChange = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MeterReadings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CustomerId = table.Column<int>(type: "INTEGER", nullable: false),
                    MeterNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Reading = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ReadingDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReadBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Source = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    PreviousReading = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    Consumption = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    IsAnomalous = table.Column<bool>(type: "INTEGER", nullable: false),
                    ValidationNotes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeterReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeterReadings_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MonthlyBills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CustomerId = table.Column<int>(type: "INTEGER", nullable: false),
                    BillNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    BillingMonth = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    PaymentReference = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    PaidDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PresentReading = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    PreviousReading = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ACPresentReading = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    ACPreviousReading = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    BlowerFanCharge = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    GeneratorCharge = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ServiceCharge = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyBills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonthlyBills_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EntityType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Action = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Details = table.Column<string>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", nullable: true),
                    UserAgent = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TransactionId = table.Column<string>(type: "TEXT", nullable: false),
                    MonthlyBillId = table.Column<int>(type: "INTEGER", nullable: false),
                    CustomerId = table.Column<int>(type: "INTEGER", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    AmountPaid = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    LatePaymentCharges = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    PaymentMethod = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    TransactionReference = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentRecords_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentRecords_MonthlyBills_MonthlyBillId",
                        column: x => x.MonthlyBillId,
                        principalTable: "MonthlyBills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrintJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JobId = table.Column<string>(type: "TEXT", nullable: false),
                    BillId = table.Column<int>(type: "INTEGER", nullable: false),
                    DocumentType = table.Column<string>(type: "TEXT", nullable: false),
                    DocumentId = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    RetryCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrintJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrintJobs_MonthlyBills_BillId",
                        column: x => x.BillId,
                        principalTable: "MonthlyBills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Email", "FirstName", "IsActive", "LastLoginAt", "LastModifiedAt", "LastModifiedBy", "LastName", "PasswordHash", "PasswordSalt", "PhoneNumber", "RequirePasswordChange", "Role", "UpdatedAt", "Username" },
                values: new object[] { 1, new DateTime(2025, 1, 17, 11, 38, 50, 558, DateTimeKind.Utc).AddTicks(2487), "system", "admin@dtcbilling.com", "System", true, null, new DateTime(2025, 1, 17, 11, 38, 50, 558, DateTimeKind.Utc).AddTicks(2490), "system", "Administrator", new byte[] { 54, 195, 218, 156, 201, 167, 150, 110, 216, 154, 191, 64, 33, 128, 249, 87, 17, 93, 83, 24, 0, 125, 94, 209, 164, 22, 191, 13, 246, 81, 39, 40 }, new byte[] { 86, 126, 46, 95, 41, 169, 1, 96, 189, 181, 104, 90, 155, 63, 19, 127 }, null, false, 0, null, "ad" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingRates_CustomerType_IsActive",
                table: "BillingRates",
                columns: new[] { "CustomerType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MeterReadings_CustomerId",
                table: "MeterReadings",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyBills_CustomerId",
                table: "MonthlyBills",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRecords_CustomerId",
                table: "PaymentRecords",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRecords_MonthlyBillId",
                table: "PaymentRecords",
                column: "MonthlyBillId");

            migrationBuilder.CreateIndex(
                name: "IX_PrintJobs_BillId",
                table: "PrintJobs",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "BillingRates");

            migrationBuilder.DropTable(
                name: "MeterReadings");

            migrationBuilder.DropTable(
                name: "PaymentRecords");

            migrationBuilder.DropTable(
                name: "PrintJobs");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "MonthlyBills");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
