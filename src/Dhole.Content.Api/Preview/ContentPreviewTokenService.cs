using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Dhole.Content.Api.Preview;

public sealed class ContentPreviewTokenService
{
    public const int DefaultExpirationMinutes = 15;
    public const int MaxExpirationMinutes = 60;

    private const int TokenVersion = 1;
    private static readonly TimeSpan ClockSkew = TimeSpan.FromMinutes(1);

    private readonly byte[] _signingKey;
    private readonly TimeProvider _timeProvider;

    public ContentPreviewTokenService(string signingSecret, TimeProvider? timeProvider = null)
    {
        if (string.IsNullOrWhiteSpace(signingSecret))
        {
            throw new ArgumentException("A preview signing secret is required.", nameof(signingSecret));
        }

        // Derive a purpose-specific key so preview tokens cannot be confused with authentication JWTs
        // even when the deployment falls back to Auth:Jwt:SecretKey.
        _signingKey = SHA256.HashData(
            Encoding.UTF8.GetBytes($"Dhole.Content.Preview.v1|{signingSecret}"));
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public IssuedPreviewToken Create(Guid contentId, Guid? issuedByUserId, int expiresInMinutes)
    {
        if (contentId == Guid.Empty)
        {
            throw new ArgumentException("Content id is required.", nameof(contentId));
        }

        if (expiresInMinutes is < 1 or > MaxExpirationMinutes)
        {
            throw new ArgumentOutOfRangeException(
                nameof(expiresInMinutes),
                $"Preview expiration must be between 1 and {MaxExpirationMinutes} minutes.");
        }

        var issuedAt = _timeProvider.GetUtcNow();
        var expiresAt = issuedAt.AddMinutes(expiresInMinutes);
        var payload = new PreviewTokenPayload(
            TokenVersion,
            contentId,
            issuedAt.ToUnixTimeSeconds(),
            expiresAt.ToUnixTimeSeconds(),
            issuedByUserId,
            Guid.NewGuid());

        var payloadBytes = JsonSerializer.SerializeToUtf8Bytes(payload);
        var payloadPart = Base64UrlEncode(payloadBytes);
        var signature = Sign(payloadPart);
        return new IssuedPreviewToken(
            $"{payloadPart}.{Base64UrlEncode(signature)}",
            contentId,
            expiresAt,
            issuedByUserId);
    }

    public bool TryValidate(string? token, out ValidatedPreviewToken validated)
    {
        validated = default!;
        if (string.IsNullOrWhiteSpace(token) || token.Length > 4096)
        {
            return false;
        }

        var separator = token.IndexOf('.');
        if (separator <= 0 || separator != token.LastIndexOf('.') || separator == token.Length - 1)
        {
            return false;
        }

        var payloadPart = token[..separator];
        var signaturePart = token[(separator + 1)..];

        try
        {
            var providedSignature = Base64UrlDecode(signaturePart);
            var expectedSignature = Sign(payloadPart);
            if (providedSignature.Length != expectedSignature.Length ||
                !CryptographicOperations.FixedTimeEquals(providedSignature, expectedSignature))
            {
                return false;
            }

            var payload = JsonSerializer.Deserialize<PreviewTokenPayload>(Base64UrlDecode(payloadPart));
            if (payload is null || payload.Version != TokenVersion || payload.ContentId == Guid.Empty)
            {
                return false;
            }

            var now = _timeProvider.GetUtcNow();
            var issuedAt = DateTimeOffset.FromUnixTimeSeconds(payload.IssuedAtUnixSeconds);
            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(payload.ExpiresAtUnixSeconds);

            if (expiresAt <= now || issuedAt > now.Add(ClockSkew) || expiresAt <= issuedAt ||
                expiresAt - issuedAt > TimeSpan.FromMinutes(MaxExpirationMinutes))
            {
                return false;
            }

            validated = new ValidatedPreviewToken(
                payload.ContentId,
                issuedAt,
                expiresAt,
                payload.IssuedByUserId,
                payload.Nonce);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    private byte[] Sign(string payloadPart)
    {
        using var hmac = new HMACSHA256(_signingKey);
        return hmac.ComputeHash(Encoding.ASCII.GetBytes(payloadPart));
    }

    private static string Base64UrlEncode(byte[] value)
        => Convert.ToBase64String(value)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static byte[] Base64UrlDecode(string value)
    {
        var normalized = value.Replace('-', '+').Replace('_', '/');
        normalized = normalized.Length % 4 switch
        {
            0 => normalized,
            2 => normalized + "==",
            3 => normalized + "=",
            _ => throw new FormatException("Invalid base64url value.")
        };
        return Convert.FromBase64String(normalized);
    }

    private sealed record PreviewTokenPayload(
        int Version,
        Guid ContentId,
        long IssuedAtUnixSeconds,
        long ExpiresAtUnixSeconds,
        Guid? IssuedByUserId,
        Guid Nonce);
}

public sealed record IssuedPreviewToken(
    string Token,
    Guid ContentId,
    DateTimeOffset ExpiresAtUtc,
    Guid? IssuedByUserId);

public sealed record ValidatedPreviewToken(
    Guid ContentId,
    DateTimeOffset IssuedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    Guid? IssuedByUserId,
    Guid Nonce);
