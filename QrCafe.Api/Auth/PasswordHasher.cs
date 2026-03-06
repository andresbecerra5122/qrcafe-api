using System.Security.Cryptography;

namespace QrCafe.Api.Auth
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string hashedPassword);
    }

    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100_000;

        public string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var key = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                KeySize
            );

            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
        }

        public bool Verify(string password, string hashedPassword)
        {
            var parts = hashedPassword.Split('.');
            if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
            {
                return false;
            }

            byte[] salt;
            byte[] expectedKey;

            try
            {
                salt = Convert.FromBase64String(parts[1]);
                expectedKey = Convert.FromBase64String(parts[2]);
            }
            catch (FormatException)
            {
                return false;
            }

            var actualKey = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedKey.Length
            );

            return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
        }
    }
}
