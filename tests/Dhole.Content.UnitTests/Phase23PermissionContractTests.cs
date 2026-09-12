using Dhole.Content.Api.Authorization;

namespace Dhole.Content.UnitTests;

public sealed class Phase23PermissionContractTests
{
    private static readonly string[] Required =
    [
        "cms.navigation.edit", "cms.collections.edit", "cms.forms.view", "cms.forms.edit",
        "cms.submissions.view", "cms.leads.view", "cms.leads.edit", "cms.meetings.view",
        "cms.meetings.edit", "cms.campaigns.view", "cms.campaigns.edit", "cms.redirects.edit",
        "cms.reviews.submit", "cms.reviews.approve",
    ];

    [Fact]
    public void All_ContainsPhase23ScopesAndLegacyScopes()
    {
        foreach (var scope in Required) Assert.Contains(scope, ContentScopeNames.All);
        Assert.Contains(ContentScopeNames.View, ContentScopeNames.All);
        Assert.Contains(ContentScopeNames.Edit, ContentScopeNames.All);
        Assert.Contains(ContentScopeNames.Publish, ContentScopeNames.All);
    }
}
