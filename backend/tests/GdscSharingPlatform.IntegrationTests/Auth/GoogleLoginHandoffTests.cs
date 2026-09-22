using System.Security.Cryptography;
using System.Text;
using GdscSharingPlatform.Api.Authentication;
using GdscSharingPlatform.Application.Features.Auth.Models;
using Microsoft.AspNetCore.WebUtilities;

namespace GdscSharingPlatform.IntegrationTests.Auth;

public sealed class GoogleLoginHandoffTests
{
    private static readonly string Verifier = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
    private static readonly string Challenge = WebEncoders.Base64UrlEncode(SHA256.HashData(Encoding.ASCII.GetBytes(Verifier)));
    private static readonly AuthResponse Session = new("test-access", "test-refresh", "Bearer", 900,
        new CurrentUserDto(Guid.NewGuid(), "member@example.test", "Member", null, null, null, "Active", null, ["Member"]));

    [Fact]
    public void Redeem_RequiresCorrectVerifier_AndIsSingleUse()
    {
        var store = new GoogleLoginHandoffStore(TimeProvider.System);
        var code = store.Issue(Session, Challenge);
        Assert.Null(store.Redeem(code, new string('a', 43)));
        Assert.Same(Session, store.Redeem(code, Verifier));
        Assert.Null(store.Redeem(code, Verifier));
    }

    [Fact]
    public async Task ConcurrentRedemptions_OnlyOneSucceeds()
    {
        var store = new GoogleLoginHandoffStore(TimeProvider.System);
        var code = store.Issue(Session, Challenge);
        var results = await Task.WhenAll(Enumerable.Range(0, 20)
            .Select(_ => Task.Run(() => store.Redeem(code, Verifier))));
        Assert.Single(results, result => result is not null);
    }

    [Fact]
    public void ExpiredCode_IsRejected()
    {
        var clock = new TestClock();
        var store = new GoogleLoginHandoffStore(clock);
        var code = store.Issue(Session, Challenge);
        clock.Now = clock.Now.AddSeconds(60);
        Assert.Null(store.Redeem(code, Verifier));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\n")]
    public void InvalidChallenge_IsRejected(string? challenge)
    {
        Assert.False(GoogleLoginHandoffStore.IsChallenge(challenge));
    }

    [Theory]
    [InlineData("https://app.example.test/api/auth/google/callback", false, true)]
    [InlineData("http://localhost:3000/api/auth/google/callback", true, true)]
    [InlineData("http://localhost:3000/api/auth/google/callback", false, false)]
    [InlineData("https://app.example.test/api/auth/google/callback?next=https://evil.test", false, false)]
    [InlineData("https://user:password@app.example.test/api/auth/google/callback", false, false)]
    [InlineData("https://app.example.test/other", false, false)]
    public void CallbackUrl_RequiresExactSafeDestination(string url, bool allowHttp, bool expected)
    {
        Assert.Equal(expected, GoogleBffOptions.IsValid(new() { CallbackUrl = url }, allowHttp));
    }

    private sealed class TestClock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => Now;
    }
}
