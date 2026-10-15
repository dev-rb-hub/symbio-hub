using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Symbio.API.Services
{
    public interface IPasswordService
    {
        string Hash(string password);
        bool Verify(string password, string storedHash, out bool needsRehash);
    }

    // Wraps the ASP.NET Core Identity password hasher (versioned, salted PBKDF2-HMAC-SHA512) so the
    // hash format and algorithm are framework-maintained. The only local logic is accepting
    // legacy unsalted SHA-256 hashes so existing accounts can be upgraded on next login.
    public sealed partial class PasswordService : IPasswordService
    {
        private readonly PasswordHasher<object> _hasher;
        private static readonly object Subject = new();

        [GeneratedRegex("^[0-9a-fA-F]{64}$")]
        private static partial Regex LegacySha256Hex();

        public PasswordService(IOptions<PasswordHasherOptions> options)
        {
            _hasher = new PasswordHasher<object>(options);
        }

        public string Hash(string password) => _hasher.HashPassword(Subject, password ?? string.Empty);

        public bool Verify(string password, string storedHash, out bool needsRehash)
        {
            needsRehash = false;
            if (string.IsNullOrEmpty(storedHash))
            {
                return false;
            }

            if (LegacySha256Hex().IsMatch(storedHash))
            {
                var candidate = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password ?? string.Empty)));
                var matches = CryptographicOperations.FixedTimeEquals(
                    Encoding.ASCII.GetBytes(candidate.ToLowerInvariant()),
                    Encoding.ASCII.GetBytes(storedHash.ToLowerInvariant()));
                needsRehash = matches;
                return matches;
            }

            try
            {
                var result = _hasher.VerifyHashedPassword(Subject, storedHash, password ?? string.Empty);
                needsRehash = result == PasswordVerificationResult.SuccessRehashNeeded;
                return result != PasswordVerificationResult.Failed;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
