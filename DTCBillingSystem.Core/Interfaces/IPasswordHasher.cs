namespace DTCBillingSystem.Core.Interfaces
{
    public interface IPasswordHasher
    {
        (byte[] Hash, byte[] Salt) HashPassword(string password);
        bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt);
    }
} 