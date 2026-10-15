using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Symbio.API.Services;
using Xunit;

namespace Symbio.API.Tests;

public sealed class PasswordServiceTests
{
    private static PasswordService Create(int iterations = 210_000) =>
        new(Options.Create(new PasswordHasherOptions { IterationCount = iterations }));

    [Fact]
    public void Hash_IsSalted_AndVerifies()
    {
        var service = Create();
        var first = service.Hash("correct horse");
        var second = service.Hash("correct horse");

        Assert.NotEqual(first, second);
        Assert.True(service.Verify("correct horse", first, out var needsRehash));
        Assert.False(needsRehash);
        Assert.False(service.Verify("wrong", first, out _));
    }

    [Fact]
    public void Verify_FlagsRehash_WhenIterationCountIncreases()
    {
        var oldHash = Create(100_000).Hash("password123");

        Assert.True(Create(210_000).Verify("password123", oldHash, out var needsRehash));
        Assert.True(needsRehash);
    }

    [Fact]
    public void Verify_AcceptsLegacySha256_AndFlagsRehash()
    {
        var legacy = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("password123"))).ToLowerInvariant();
        var service = Create();

        Assert.True(service.Verify("password123", legacy, out var needsRehash));
        Assert.True(needsRehash);
        Assert.False(service.Verify("nope", legacy, out _));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-hash")]
    [InlineData("AQAAAAEAA!!!")]
    public void Verify_RejectsMalformedHashes(string stored)
    {
        Assert.False(Create().Verify("password123", stored, out _));
    }
}
