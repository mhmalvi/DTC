using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using DTCBillingSystem.Core.Services;

namespace DTCBillingSystem.UI
{
    public class Program
    {
        [STAThread]
        public static async Task Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--reset-admin-password")
            {
                await ResetAdminPassword();
                return;
            }

            var app = new App();
            app.InitializeComponent();
            app.Run();
        }

        private static async Task ResetAdminPassword()
        {
            try
            {
                var passwordHasher = new PasswordHasher();
                var newPassword = "Admin@123";
                var (hash, salt) = passwordHasher.HashPassword(newPassword);

                using var connection = new SqliteConnection("Data Source=dtcbilling.db");
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Users 
                    SET PasswordHash = @hash, 
                        PasswordSalt = @salt,
                        RequirePasswordChange = 1,
                        LastModifiedAt = @lastModified,
                        LastModifiedBy = 'system'
                    WHERE Username = 'admin'";

                command.Parameters.AddWithValue("@hash", hash);
                command.Parameters.AddWithValue("@salt", salt);
                command.Parameters.AddWithValue("@lastModified", DateTime.UtcNow.ToString("O"));

                var rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected > 0)
                {
                    Console.WriteLine($"Admin password has been reset to: {newPassword}");
                    Console.WriteLine("Please change this password after logging in.");
                }
                else
                {
                    Console.WriteLine("Admin user not found in the database.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting admin password: {ex.Message}");
            }
        }
    }
} 