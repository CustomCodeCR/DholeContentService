using Dhole.Content.Api.Preview;
using Xunit;

namespace Dhole.Content.UnitTests;

public sealed class Phase30PreviewTests
{
    private const string SigningSecret = "phase30-preview-test-secret-with-more-than-32-characters";

    [Fact]
    public void PreviewToken_RoundTripsContentAndIssuer()
    {
        var now = new DateTimeOffset(2026, 9, 13, 21, 30, 0, TimeSpan.Zero);
        var clock = new TestTimeProvider(now);
        var service = new ContentPreviewTokenService(SigningSecret, clock);
        var contentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var issued = service.Create(contentId, userId, 15);

        Assert.True(service.TryValidate(issued.Token, out var validated));
        Assert.Equal(contentId, validated.ContentId);
        Assert.Equal(userId, validated.IssuedByUserId);
        Assert.Equal(now, validated.IssuedAtUtc);
        Assert.Equal(now.AddMinutes(15), validated.ExpiresAtUtc);
        Assert.NotEqual(Guid.Empty, validated.Nonce);
    }

    [Fact]
    public void PreviewToken_RejectsExpiredToken()
    {
        var clock = new TestTimeProvider(new DateTimeOffset(2026, 9, 13, 21, 30, 0, TimeSpan.Zero));
        var service = new ContentPreviewTokenService(SigningSecret, clock);
        var issued = service.Create(Guid.NewGuid(), Guid.NewGuid(), 5);

        clock.Advance(TimeSpan.FromMinutes(6));

        Assert.False(service.TryValidate(issued.Token, out _));
    }

    [Fact]
    public void PreviewToken_RejectsTampering()
    {
        var service = new ContentPreviewTokenService(SigningSecret);
        var issued = service.Create(Guid.NewGuid(), Guid.NewGuid(), 15);
        var replacement = issued.Token[^1] == 'A' ? 'B' : 'A';
        var tampered = issued.Token[..^1] + replacement;

        Assert.False(service.TryValidate(tampered, out _));
    }

    [Fact]
    public void PreviewToken_CannotBeValidatedWithDifferentSecret()
    {
        var issuer = new ContentPreviewTokenService(SigningSecret);
        var validator = new ContentPreviewTokenService("a-completely-different-preview-signing-secret-123456");
        var issued = issuer.Create(Guid.NewGuid(), Guid.NewGuid(), 15);

        Assert.False(validator.TryValidate(issued.Token, out _));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(61)]
    public void PreviewToken_RejectsInvalidLifetime(int minutes)
    {
        var service = new ContentPreviewTokenService(SigningSecret);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            service.Create(Guid.NewGuid(), Guid.NewGuid(), minutes));
    }

    [Fact]
    public void PreviewToken_IsUniqueAcrossMultipleLinksForSameContent()
    {
        var clock = new TestTimeProvider(new DateTimeOffset(2026, 9, 13, 21, 30, 0, TimeSpan.Zero));
        var service = new ContentPreviewTokenService(SigningSecret, clock);
        var contentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var first = service.Create(contentId, userId, 15);
        var second = service.Create(contentId, userId, 15);

        Assert.NotEqual(first.Token, second.Token);
    }

    private sealed class TestTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private DateTimeOffset _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;

        public void Advance(TimeSpan value) => _utcNow = _utcNow.Add(value);
    }
}
