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
    public static readonly Error MenuNotFound=new("Content.MenuNotFound","No se encontró el menú solicitado.");
    public static readonly Error SettingNotFound=new("Content.SettingNotFound","No se encontró la configuración solicitada.");
    public static readonly Error InvalidJson=new("Content.InvalidJson","El contenido JSON no es válido.");
    public static readonly Error SiteNotFound=new("Content.SiteNotFound","No se encontró el sitio solicitado.");
    public static readonly Error SiteKeyAlreadyExists=new("Content.SiteKeyAlreadyExists","Ya existe un sitio con el mismo SiteKey.");
    public static readonly Error SiteDomainAlreadyExists=new("Content.SiteDomainAlreadyExists","Ya existe un sitio con el mismo dominio principal.");
    public static readonly Error InvalidSiteData=new("Content.InvalidSiteData","Los datos del sitio no son válidos.");
}
