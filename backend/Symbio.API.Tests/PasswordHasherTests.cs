using System.Security.Cryptography;
using System.Text;
using Symbio.API.Services;
using Xunit;

namespace Symbio.API.Tests;

public sealed class PasswordHasherTests
{
    [Fact]
    public void Hash_IsSalted_AndVerifies()
    {
        var first = PasswordHasher.Hash("correct horse");
        var second = PasswordHasher.Hash("correct horse");

        Assert.NotEqual(first, second);
        Assert.True(PasswordHasher.Verify("correct horse", first, out var needsRehash));
        Assert.False(needsRehash);
        Assert.False(PasswordHasher.Verify("wrong", first, out _));
    }

    [Fact]
    public void Verify_AcceptsLegacySha256_AndFlagsRehash()
    {
        var legacy = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("password123"))).ToLowerInvariant();

        Assert.True(PasswordHasher.Verify("password123", legacy, out var needsRehash));
        Assert.True(needsRehash);
        Assert.False(PasswordHasher.Verify("nope", legacy, out _));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-hash")]
    [InlineData("pbkdf2-sha256$abc$!!$!!")]
    public void Verify_RejectsMalformedHashes(string stored)
    {
        Assert.False(PasswordHasher.Verify("password123", stored, out _));
    }
}
