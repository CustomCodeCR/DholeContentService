using CustomCodeFramework.Core.Results;

namespace Dhole.Content.Domain.Shared;

public static class CampaignErrors
{
    public static readonly Error NotFound = new("Content.MarketingCampaignNotFound", "No se encontró la campaña solicitada.");
    public static readonly Error SlugAlreadyExists = new("Content.MarketingCampaignSlugAlreadyExists", "Ya existe una campaña con el mismo slug para este sitio.");
    public static readonly Error InvalidData = new("Content.InvalidMarketingCampaignData", "Los datos de la campaña no son válidos.");
}
