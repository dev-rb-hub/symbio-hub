using System.Security.Cryptography;
using System.Text;

namespace Symbio.API.Services
{
    public static class PasswordHasher
    {
        private const string Prefix = "pbkdf2-sha256";
        private const int Iterations = 600_000;
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int LegacySha256HexLength = 64;

        // Format: pbkdf2-sha256$<iterations>$<base64 salt>$<base64 hash>
        public static string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password ?? string.Empty, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
            return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        // Also accepts legacy unsalted SHA-256 hashes so existing accounts keep working until they are re-hashed on login.
        public static bool Verify(string password, string storedHash, out bool needsRehash)
        {
            needsRehash = false;
            if (string.IsNullOrEmpty(storedHash))
            {
                return false;
            }

            if (storedHash.StartsWith(Prefix + "$", StringComparison.Ordinal))
            {
                var parts = storedHash.Split('$');
                if (parts.Length != 4 || !int.TryParse(parts[1], out var iterations) || iterations <= 0)
                {
                    return false;
                }

                try
                {
                    var salt = Convert.FromBase64String(parts[2]);
                    var expected = Convert.FromBase64String(parts[3]);
                    var actual = Rfc2898DeriveBytes.Pbkdf2(password ?? string.Empty, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
                    var matches = CryptographicOperations.FixedTimeEquals(actual, expected);
                    needsRehash = matches && iterations < Iterations;
                    return matches;
                }
                catch (FormatException)
                {
                    return false;
                }
            }

            if (storedHash.Length == LegacySha256HexLength)
            {
                var legacy = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password ?? string.Empty))).ToLowerInvariant();
                var matches = CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(legacy), Encoding.ASCII.GetBytes(storedHash.ToLowerInvariant()));
                needsRehash = matches;
                return matches;
            }

            return false;
        }
    }
}
