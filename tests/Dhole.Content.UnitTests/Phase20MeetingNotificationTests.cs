using Dhole.Content.Application.Meetings;
using Dhole.Content.Domain.Forms.Entities;
using Xunit;

namespace Dhole.Content.UnitTests;

public sealed class Phase20MeetingNotificationTests
{
    [Fact]
    public void ResolveSubmissionEmail_UsesDeclaredEmailField()
    {
        var formId = Guid.NewGuid();
        var fields = new[]
        {
            MarketingFormField.Create(formId, "name", "Nombre", "text", null, true, 0, null, null, null),
            MarketingFormField.Create(formId, "contact-email", "Correo", "email", null, true, 1, null, null, null)
        };

        var email = MeetingNotificationIntegration.ResolveSubmissionEmail(
            "{\"name\":\"Ana\",\"contact-email\":\" ANA@EXAMPLE.COM \"}", fields);

        Assert.Equal("ana@example.com", email);
    }

    [Fact]
    public void ResolveSubmissionEmail_DoesNotGuessUndeclaredEmailFields()
    {
        var formId = Guid.NewGuid();
        var fields = new[]
        {
            MarketingFormField.Create(formId, "contact", "Contacto", "text", null, false, 0, null, null, null)
        };

        var email = MeetingNotificationIntegration.ResolveSubmissionEmail(
            "{\"email\":\"ana@example.com\",\"contact\":\"ana@example.com\"}", fields);

        Assert.Null(email);
    }

    [Fact]
    public void MeetingNotificationEvents_UseStableIntegrationNames()
    {
        Assert.Equal("content.meeting.requested", MeetingNotificationIntegration.RequestedEventName);
        Assert.Equal("content.meeting.confirmed", MeetingNotificationIntegration.ConfirmedEventName);
    }
}
