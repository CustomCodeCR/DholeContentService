using CustomCodeFramework.Core.Results;
namespace Dhole.Content.Domain.Shared;
public static class ContentErrors
{
    public static readonly Error ContentNotFound=new("Content.ContentNotFound","No se encontró el contenido solicitado.");
    public static readonly Error ContentSlugAlreadyExists=new("Content.ContentSlugAlreadyExists","Ya existe contenido con el mismo slug para este sitio.");
    public static readonly Error InvalidContentType=new("Content.InvalidContentType","El tipo de contenido no es válido.");
    public static readonly Error InvalidContentState=new("Content.InvalidContentState","El estado actual no permite realizar esta operación.");
    public static readonly Error InvalidScheduleDate=new("Content.InvalidScheduleDate","La fecha programada debe estar en el futuro.");
    public static readonly Error RevisionNotFound=new("Content.RevisionNotFound","No se encontró la revisión solicitada.");
    public static readonly Error TaxonomyNotFound=new("Content.TaxonomyNotFound","No se encontró la categoría o etiqueta solicitada.");
    public static readonly Error TaxonomySlugAlreadyExists=new("Content.TaxonomySlugAlreadyExists","Ya existe una categoría o etiqueta con ese slug.");
    public static readonly Error MediaNotFound=new("Content.MediaNotFound","No se encontró el recurso multimedia solicitado.");
    public static readonly Error StorageFileAlreadyRegistered=new("Content.StorageFileAlreadyRegistered","El archivo ya está registrado en la biblioteca multimedia.");
    public static readonly Error ContentMediaNotFound=new("Content.ContentMediaNotFound","No se encontró la asociación multimedia solicitada.");
    public static readonly Error ContentMediaAlreadyExists=new("Content.ContentMediaAlreadyExists","El recurso multimedia ya está asociado al contenido con ese rol.");
    public static readonly Error InvalidContentMedia=new("Content.InvalidContentMedia","Los datos de la asociación multimedia no son válidos.");
    public static readonly Error MenuNotFound=new("Content.MenuNotFound","No se encontró el menú solicitado.");
    public static readonly Error SettingNotFound=new("Content.SettingNotFound","No se encontró la configuración solicitada.");
    public static readonly Error InvalidJson=new("Content.InvalidJson","El contenido JSON no es válido.");
    public static readonly Error SiteNotFound=new("Content.SiteNotFound","No se encontró el sitio solicitado.");
    public static readonly Error SiteKeyAlreadyExists=new("Content.SiteKeyAlreadyExists","Ya existe un sitio con el mismo SiteKey.");
    public static readonly Error SiteDomainAlreadyExists=new("Content.SiteDomainAlreadyExists","Ya existe un sitio con el mismo dominio principal.");
    public static readonly Error InvalidSiteData=new("Content.InvalidSiteData","Los datos del sitio no son válidos.");
    public static readonly Error ContentRouteNotFound=new("Content.ContentRouteNotFound","No se encontró la ruta de contenido solicitada.");
    public static readonly Error ContentRoutePathAlreadyExists=new("Content.ContentRoutePathAlreadyExists","Ya existe una ruta con el mismo path para este sitio e idioma.");
    public static readonly Error ContentRouteSiteMismatch=new("Content.ContentRouteSiteMismatch","La ruta y el contenido deben pertenecer al mismo sitio e idioma.");
    public static readonly Error InvalidBlocksJson=new("Content.InvalidBlocksJson","BlocksJson no cumple el formato seguro del Page Builder.");
    public static readonly Error PageBuilderOnlyPages=new("Content.PageBuilderOnlyPages","El Page Builder solo puede utilizarse con contenido de tipo Page.");
    public static readonly Error PageBuilderBlockNotFound=new("Content.PageBuilderBlockNotFound","No se encontró el bloque solicitado.");
    public static readonly Error InvalidPageBuilderOperation=new("Content.InvalidPageBuilderOperation","La operación o los datos del Page Builder no son válidos o contienen contenido no permitido.");
    public static readonly Error PlacementNotFound=new("Content.PlacementNotFound","No se encontró el placement solicitado.");
    public static readonly Error PlacementCodeAlreadyExists=new("Content.PlacementCodeAlreadyExists","Ya existe un placement con el mismo código para este sitio.");
    public static readonly Error PlacementItemNotFound=new("Content.PlacementItemNotFound","No se encontró el contenido asociado al placement.");
    public static readonly Error PlacementItemAlreadyExists=new("Content.PlacementItemAlreadyExists","El contenido ya está asociado a este placement.");
    public static readonly Error PlacementContentNotAllowed=new("Content.PlacementContentNotAllowed","El contenido no pertenece al mismo sitio o su tipo no está permitido por el placement.");
    public static readonly Error InvalidPlacementData=new("Content.InvalidPlacementData","Los datos del placement o de su contenido asociado no son válidos.");
}
