using System;
using System.Security.Cryptography;
using DTCBillingSystem.Core.Interfaces;

namespace DTCBillingSystem.Core.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 350000;
        private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

        public (byte[] Hash, byte[] Salt) HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithm,
                HashSize);

            return (hash, salt);
        }

        public bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            var computedHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                storedSalt,
                Iterations,
                HashAlgorithm,
                HashSize);

            return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
        }
    }
} 