using Dhole.Content.Domain.Forms;
using Dhole.Content.Domain.Forms.Entities;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dhole.Content.UnitTests;

public sealed class Phase15MarketingFormTests
{
    [Fact]
    public void MarketingForm_Create_NormalizesDataAndStartsAtVersionOne()
    {
        var form = MarketingForm.Create(
            " MAIN ",
            " Contact.Home ",
            "Contacto principal",
            "CONTACT",
            "active",
            "Gracias por contactarnos.",
            "marketing.contact.received",
            "{\"layout\":\"compact\"}",
            Guid.NewGuid());

        Assert.Equal("main", form.SiteKey);
        Assert.Equal("contact.home", form.FormKey);
        Assert.Equal(MarketingFormRules.PurposeContact, form.Purpose);
        Assert.Equal(MarketingFormRules.StatusActive, form.Status);
        Assert.Equal(1, form.Version);
    }

    [Fact]
    public void MarketingForm_Update_IncrementsVersion()
    {
        var form = MarketingForm.Create("main", "newsletter", "Newsletter",
            MarketingFormRules.PurposeNewsletter, MarketingFormRules.StatusDraft, null, null, null, null);

        form.Update("main", "newsletter", "Newsletter web", MarketingFormRules.PurposeNewsletter,
            MarketingFormRules.StatusActive, "Listo", null, "{}", null);

        Assert.Equal(2, form.Version);
        Assert.Equal("Newsletter web", form.Name);
    }

    [Fact]
    public void MarketingForm_FieldChange_IncrementsVersion()
    {
        var form = MarketingForm.Create("main", "campaign.lead", "Campaña",
            MarketingFormRules.PurposeCampaign, MarketingFormRules.StatusDraft, null, null, null, null);

        form.TouchFieldChange(null);

        Assert.Equal(2, form.Version);
    }

    [Fact]
    public void MarketingFormRules_ExposeExpectedInitialPurposes()
    {
        Assert.Equal(
            ["contact", "quote-request", "meeting", "newsletter", "campaign"],
            MarketingFormRules.InitialPurposes.ToArray());
    }

    [Fact]
    public void MarketingForm_RejectsUnsupportedPurpose()
        => Assert.Throws<ArgumentException>(() => MarketingForm.Create(
            "main", "custom", "Custom", "other", MarketingFormRules.StatusDraft, null, null, null, null));

    [Theory]
    [InlineData("[]")]
    [InlineData("\"text\"")]
    [InlineData("not-json")]
    public void MarketingForm_RejectsInvalidSettingsJson(string settingsJson)
        => Assert.Throws<ArgumentException>(() => MarketingForm.Create(
            "main", "contact", "Contacto", MarketingFormRules.PurposeContact,
            MarketingFormRules.StatusDraft, null, null, settingsJson, null));

    [Fact]
    public void MarketingFormField_Create_NormalizesKeysAndJson()
    {
        var field = MarketingFormField.Create(
            Guid.NewGuid(),
            " Customer.Email ",
            "Correo electrónico",
            " EMAIL ",
            "nombre@empresa.com",
            true,
            2,
            "{\"format\":\"email\"}",
            "[{\"value\":\"business\",\"label\":\"Empresa\"}]",
            null);

        Assert.Equal("customer.email", field.FieldKey);
        Assert.Equal("email", field.FieldType);
        Assert.Equal(2, field.SortOrder);
        Assert.True(field.IsRequired);
        Assert.Equal("{\"format\":\"email\"}", field.ValidationJson);
        Assert.StartsWith("[", field.OptionsJson);
    }

    [Theory]
    [InlineData("[]")]
    [InlineData("not-json")]
    public void MarketingFormField_RejectsInvalidValidationJson(string validationJson)
        => Assert.Throws<ArgumentException>(() => MarketingFormField.Create(
            Guid.NewGuid(), "email", "Email", "email", null, true, 0, validationJson, null, null));

    [Fact]
    public void MarketingFormField_RejectsObjectOptionsJson()
        => Assert.Throws<ArgumentException>(() => MarketingFormField.Create(
            Guid.NewGuid(), "service", "Servicio", "select", null, false, 0, null, "{}", null));

    [Fact]
    public void MarketingFormField_RejectsNegativeSortOrder()
        => Assert.Throws<ArgumentOutOfRangeException>(() => MarketingFormField.Create(
            Guid.NewGuid(), "email", "Email", "email", null, true, -1, null, null, null));

    [Fact]
    public void MarketingFormModel_HasExpectedIndexesAndForeignKey()
    {
        var options = new DbContextOptionsBuilder<ServiceDbContext>()
            .UseNpgsql("Host=localhost;Database=dhole_content_phase15_test")
            .Options;
        using var db = new ServiceDbContext(options);

        var form = db.Model.FindEntityType(typeof(MarketingForm));
        var field = db.Model.FindEntityType(typeof(MarketingFormField));
        Assert.NotNull(form);
        Assert.NotNull(field);

        var uniqueFormKey = form!.GetIndexes().Single(index =>
            index.Properties.Select(property => property.Name).SequenceEqual(["SiteKey", "FormKey"]));
        Assert.True(uniqueFormKey.IsUnique);
        Assert.Equal("is_deleted = false", uniqueFormKey.GetFilter());

        var uniqueFieldKey = field!.GetIndexes().Single(index =>
            index.Properties.Select(property => property.Name).SequenceEqual(["FormId", "FieldKey"]));
        Assert.True(uniqueFieldKey.IsUnique);
        Assert.Equal("is_deleted = false", uniqueFieldKey.GetFilter());

        var sortIndex = field.GetIndexes().Single(index =>
            index.Properties.Select(property => property.Name).SequenceEqual(["FormId", "SortOrder"]));
        Assert.False(sortIndex.IsUnique);
        Assert.Equal("is_deleted = false", sortIndex.GetFilter());

        Assert.Contains(field.GetForeignKeys(), foreignKey =>
            foreignKey.Properties.Select(property => property.Name).SequenceEqual(["FormId"]));
    }

    [Fact]
    public void ServiceDbContext_ExposesMarketingForms()
    {
        var propertyNames = typeof(ServiceDbContext).GetProperties()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("MarketingForms", propertyNames);
        Assert.Contains("MarketingFormFields", propertyNames);
    }
}
