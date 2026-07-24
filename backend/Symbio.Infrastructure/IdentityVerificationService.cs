using System.Text.RegularExpressions;
using Symbio.Core.Repositories;

namespace Symbio.Infrastructure;

public class IdentityVerificationService : IIdentityVerificationService
{
    private static readonly Regex DigitsOnly = new("^[0-9]{11}$", RegexOptions.Compiled);

    public Task<bool> ValidateBusinessIdentifierAsync(string businessIdentifier, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(businessIdentifier))
        {
            return Task.FromResult(false);
        }

        var normalized = businessIdentifier.Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase);
        var isValid = DigitsOnly.IsMatch(normalized);

        return Task.FromResult(isValid);
    }
}