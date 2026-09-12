using System.Text.Json;
using Dhole.Content.Domain.ContentItems.Entities;
using Dhole.Content.Domain.Forms.Entities;
using Dhole.Content.Domain.Submissions;
using Dhole.Content.Domain.Submissions.Entities;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dhole.Content.UnitTests;

public sealed class Phase16MarketingSubmissionTests
{
    [Fact]
    public void SubmissionRules_HashIp_ReturnsSha256WithoutKeepingRawIp()
    {
        const string rawIp = "192.0.2.10";

        var hash = SubmissionRules.HashIp(rawIp);

        Assert.NotNull(hash);
        Assert.Equal(64, hash!.Length);
        Assert.DoesNotContain(rawIp, hash, StringComparison.Ordinal);
        Assert.All(hash, character => Assert.True(Uri.IsHexDigit(character)));
    }

    [Fact]
    public void SubmissionRules_ValidateAndSanitizePayload_UsesDeclaredCanonicalKeys()
    {
        var payload = SubmissionRules.ValidateAndSanitizePayload(
            "{\"Email\":\"cliente@example.com\",\"message\":\"Hola\"}",
            [("email", true), ("message", false)]);

        using var document = JsonDocument.Parse(payload);
        Assert.Equal("cliente@example.com", document.RootElement.GetProperty("email").GetString());
        Assert.Equal("Hola", document.RootElement.GetProperty("message").GetString());
        Assert.False(document.RootElement.TryGetProperty("Email", out _));
    }

    [Fact]
    public void SubmissionRules_RejectsUnknownPayloadFields()
        => Assert.Throws<ArgumentException>(() => SubmissionRules.ValidateAndSanitizePayload(
            "{\"email\":\"cliente@example.com\",\"password\":\"secret\"}",
            [("email", true)]));

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"email\":null}")]
    [InlineData("{\"email\":\"   \"}")]
    public void SubmissionRules_RejectsMissingRequiredFields(string payload)
        => Assert.Throws<ArgumentException>(() => SubmissionRules.ValidateAndSanitizePayload(
            payload,
            [("email", true)]));

    [Fact]
    public void MarketingSubmission_Create_PersistsExpectedContext()
    {
        var formId = Guid.NewGuid();
        var contentId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();
        var submittedAt = DateTime.UtcNow;
        var hash = SubmissionRules.HashIp("203.0.113.15");

        var submission = MarketingSubmission.Create(
            formId,
            contentId,
            campaignId,
            submittedAt,
            "https://example.com/contacto",
            "https://example.com/",
            "google",
            "cpc",
            "septiembre",
            "hero",
            "logistica",
            "{\"email\":\"cliente@example.com\"}",
            hash,
            "Mozilla/5.0",
            correlationId);

        Assert.Equal(formId, submission.FormId);
        Assert.Equal(contentId, submission.ContentId);
        Assert.Equal(campaignId, submission.CampaignId);
        Assert.Equal(SubmissionRules.StatusReceived, submission.Status);
        Assert.Equal(submittedAt, submission.SubmittedAtUtc);
        Assert.Equal(hash, submission.IpHash);
        Assert.Equal(correlationId, submission.CorrelationId);
    }

    [Fact]
    public void MarketingSubmission_RejectsRawIpInsteadOfHash()
        => Assert.Throws<ArgumentException>(() => MarketingSubmission.Create(
            Guid.NewGuid(), null, null, DateTime.UtcNow, "https://example.com/contacto", null,
            null, null, null, null, null, "{}", "192.0.2.10", null, Guid.NewGuid()));

    [Fact]
    public void MarketingSubmissionModel_HasExpectedIndexesAndForeignKeys()
    {
        var options = new DbContextOptionsBuilder<ServiceDbContext>()
            .UseNpgsql("Host=localhost;Database=dhole_content_phase16_test")
            .Options;
        using var db = new ServiceDbContext(options);

        var submission = db.Model.FindEntityType(typeof(MarketingSubmission));
        Assert.NotNull(submission);

        var formDateIndex = submission!.GetIndexes().Single(index =>
            index.Properties.Select(property => property.Name).SequenceEqual(["FormId", "SubmittedAtUtc"]));
        Assert.False(formDateIndex.IsUnique);
        Assert.Equal("is_deleted = false", formDateIndex.GetFilter());

        var correlationIndex = submission.GetIndexes().Single(index =>
            index.Properties.Select(property => property.Name).SequenceEqual(["CorrelationId"]));
        Assert.True(correlationIndex.IsUnique);
        Assert.Equal("is_deleted = false", correlationIndex.GetFilter());

        var formForeignKey = submission.GetForeignKeys().Single(foreignKey =>
            foreignKey.Properties.Select(property => property.Name).SequenceEqual(["FormId"]));
        Assert.Equal(typeof(MarketingForm), formForeignKey.PrincipalEntityType.ClrType);
        Assert.Equal(DeleteBehavior.Restrict, formForeignKey.DeleteBehavior);

        var contentForeignKey = submission.GetForeignKeys().Single(foreignKey =>
            foreignKey.Properties.Select(property => property.Name).SequenceEqual(["ContentId"]));
        Assert.Equal(typeof(ContentItem), contentForeignKey.PrincipalEntityType.ClrType);
        Assert.Equal(DeleteBehavior.SetNull, contentForeignKey.DeleteBehavior);
    }

    [Fact]
    public void ServiceDbContext_ExposesMarketingSubmissions()
    {
        var propertyNames = typeof(ServiceDbContext).GetProperties()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("MarketingSubmissions", propertyNames);
    }
}
