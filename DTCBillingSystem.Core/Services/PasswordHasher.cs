using System;
using System.Security.Cryptography;
using DTCBillingSystem.Core.Interfaces;
using System.Diagnostics;

namespace DTCBillingSystem.Core.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int KeySize = 32; // 256 bits
        private const int Iterations = 10000;

        public (byte[] Hash, byte[] Salt) HashPassword(string password)
        {
            try
            {
                if (string.IsNullOrEmpty(password))
                    throw new ArgumentNullException(nameof(password));

                using var rng = RandomNumberGenerator.Create();
                var salt = new byte[KeySize];
                rng.GetBytes(salt);

                using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
                var hash = pbkdf2.GetBytes(KeySize);

                Debug.WriteLine($"Generated hash length: {hash.Length}, salt length: {salt.Length}");
                return (Hash: hash, Salt: salt);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error hashing password: {ex.Message}");
                throw;
            }
        }

        public bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            try
            {
                Debug.WriteLine($"Verifying password - Input length: {password?.Length ?? 0}, Hash length: {storedHash?.Length ?? 0}, Salt length: {storedSalt?.Length ?? 0}");

                if (string.IsNullOrEmpty(password))
                {
                    Debug.WriteLine("Password is null or empty");
                    return false;
                }

                if (storedHash == null || storedHash.Length == 0)
                {
                    Debug.WriteLine("Stored hash is null or empty");
                    return false;
                }

                if (storedSalt == null || storedSalt.Length == 0)
                {
                    Debug.WriteLine("Stored salt is null or empty");
                    return false;
                }

                using var pbkdf2 = new Rfc2898DeriveBytes(password, storedSalt, Iterations, HashAlgorithmName.SHA256);
                var computedHash = pbkdf2.GetBytes(KeySize);

                Debug.WriteLine($"Computed hash length: {computedHash.Length}");
                
                return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error verifying password: {ex.Message}");
                return false;
            }
        }
    }
} 