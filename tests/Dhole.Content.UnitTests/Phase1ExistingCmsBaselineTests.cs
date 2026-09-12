using Dhole.Content.Domain.ContentItems.Enums;
using Dhole.Content.Persistence.DbContexts;
using Xunit;

namespace Dhole.Content.UnitTests;

public sealed class Phase1ExistingCmsBaselineTests
{
    [Fact]
    public void ServiceDbContext_PreservesExistingCmsConcepts()
    {
        var propertyNames = typeof(ServiceDbContext)
            .GetProperties()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.Ordinal);

        string[] expectedProperties =
        [
            "ContentItems",
            "ContentRevisions",
            "ContentTaxonomies",
            "TaxonomyTerms",
            "MediaReferences",
            "NavigationMenus",
            "NavigationMenuItems",
            "SiteSettings",
            "InboxMessages",
            "OutboxMessages"
        ];

        foreach (var expectedProperty in expectedProperties)
        {
            Assert.Contains(expectedProperty, propertyNames);
        }
    }

    [Fact]
    public void ContentType_PreservesExistingContentTypes()
    {
        var contentTypes = Enum.GetNames<ContentType>().ToHashSet(StringComparer.Ordinal);

        string[] expectedContentTypes =
        [
            "Page",
            "News",
            "Post",
            "Announcement",
            "Banner",
            "Video",
            "ReusableBlock"
        ];

        foreach (var expectedContentType in expectedContentTypes)
        {
            Assert.Contains(expectedContentType, contentTypes);
        }
    }
}
