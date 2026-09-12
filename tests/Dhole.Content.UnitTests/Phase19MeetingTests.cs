using Dhole.Content.Domain.Leads.Entities;
using Dhole.Content.Domain.Meetings;
using Dhole.Content.Domain.Meetings.Entities;
using Dhole.Content.Domain.Submissions.Entities;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dhole.Content.UnitTests;

public sealed class Phase19MeetingTests
{
    [Fact]
    public void MeetingType_Create_NormalizesAndValidates()
    {
        var type = MeetingType.Create(" MAIN ", " Reunión comercial ", " Reunion-Comercial ", " Consulta ",
            30, 10, " Online ", null, " ventas ", "{\"color\":\"blue\"}", true, null);
        Assert.Equal("main", type.SiteKey);
        Assert.Equal("reunion-comercial", type.Slug);
        Assert.Equal(30, type.DurationMinutes);
        Assert.Equal(10, type.BufferMinutes);
        Assert.Equal("Online", type.MeetingMode);
        Assert.True(type.IsActive);
    }

    [Fact]
    public void MeetingType_RejectsInvalidDuration()
        => Assert.Throws<ArgumentOutOfRangeException>(() => MeetingType.Create("main", "Demo", "demo", null,
            0, 0, "Online", null, null, null, true, null));

    [Fact]
    public void MeetingRequest_DefaultsToRequestedAndSupportsLifecycle()
    {
        var start = DateTime.UtcNow.AddDays(1);
        var request = MeetingRequest.Create(Guid.NewGuid(), Guid.NewGuid(), null, start, start.AddMinutes(30),
            "America/Costa_Rica", "Consulta", null, null, null);
        Assert.Equal(MeetingRules.StatusRequested, request.Status);
        request.MarkPendingConfirmation(Guid.NewGuid(), null);
        Assert.Equal(MeetingRules.StatusPendingConfirmation, request.Status);
        request.Confirm(start.AddMinutes(5), start.AddMinutes(35), null, "calendar", "evt-1", "https://meet.example/1", null);
        Assert.Equal(MeetingRules.StatusConfirmed, request.Status);
        Assert.NotNull(request.ConfirmedStartUtc);
        request.Complete(null);
        Assert.Equal(MeetingRules.StatusCompleted, request.Status);
    }

    [Fact]
    public void MeetingRequest_RequiresLeadOrSubmission()
    {
        var start = DateTime.UtcNow.AddDays(1);
        Assert.Throws<ArgumentException>(() => MeetingRequest.Create(Guid.NewGuid(), null, null, start,
            start.AddMinutes(30), "UTC", "Demo", null, null, null));
    }

    [Fact]
    public void MeetingRequest_RejectsInvalidTransition()
    {
        var start = DateTime.UtcNow.AddDays(1);
        var request = MeetingRequest.Create(Guid.NewGuid(), Guid.NewGuid(), null, start, start.AddMinutes(30),
            "UTC", "Demo", null, null, null);
        Assert.Throws<InvalidOperationException>(() => request.Complete(null));
    }

    [Fact]
    public void MeetingModel_HasExpectedIndexesAndForeignKeys()
    {
        var options = new DbContextOptionsBuilder<ServiceDbContext>()
            .UseNpgsql("Host=localhost;Database=dhole_content_phase19_test")
            .Options;
        using var db = new ServiceDbContext(options);
        var type = db.Model.FindEntityType(typeof(MeetingType));
        var request = db.Model.FindEntityType(typeof(MeetingRequest));
        Assert.NotNull(type);
        Assert.NotNull(request);
        var uniqueSlug = type!.GetIndexes().Single(index =>
            index.Properties.Select(property => property.Name).SequenceEqual(["SiteKey", "Slug"]));
        Assert.True(uniqueSlug.IsUnique);
        Assert.Contains(request!.GetForeignKeys(), fk => fk.PrincipalEntityType.ClrType == typeof(MeetingType));
        Assert.Contains(request.GetForeignKeys(), fk => fk.PrincipalEntityType.ClrType == typeof(MarketingLead));
        Assert.Contains(request.GetForeignKeys(), fk => fk.PrincipalEntityType.ClrType == typeof(MarketingSubmission));
        Assert.DoesNotContain(request.GetForeignKeys(), fk => fk.Properties.Any(p => p.Name == "AssignedUserId"));
    }

    [Fact]
    public void ServiceDbContext_ExposesMeetings()
    {
        var names = typeof(ServiceDbContext).GetProperties().Select(x => x.Name).ToHashSet(StringComparer.Ordinal);
        Assert.Contains("MeetingTypes", names);
        Assert.Contains("MeetingRequests", names);
    }
}
