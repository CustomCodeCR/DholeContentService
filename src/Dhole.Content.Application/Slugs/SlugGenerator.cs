using System.Globalization;
using System.Text;
using Dhole.Content.Application.Abstractions.Slugs;
namespace Dhole.Content.Application.Slugs;
public sealed class SlugGenerator : ISlugGenerator
{
    public string Generate(string value)
    {
        if(string.IsNullOrWhiteSpace(value)) throw new ArgumentException("El valor para generar el slug es obligatorio.",nameof(value));
        var normalized=RemoveDiacritics(value.Trim()).ToLowerInvariant(); var b=new StringBuilder(); var separator=false;
        foreach(var c in normalized){if(char.IsLetterOrDigit(c)){b.Append(c);separator=false;}else if(!separator){b.Append('-');separator=true;}}
        var slug=b.ToString().Trim('-'); if(string.IsNullOrWhiteSpace(slug)) throw new InvalidOperationException("No fue posible generar un slug válido."); return slug;
    }
    public async Task<string> GenerateUniqueAsync(string value,Func<string,CancellationToken,Task<bool>> existsAsync,CancellationToken cancellationToken=default)
    { var root=Generate(value); if(!await existsAsync(root,cancellationToken)) return root; for(var i=2;i<=9999;i++){var slug=$"{root}-{i}";if(!await existsAsync(slug,cancellationToken))return slug;} throw new InvalidOperationException($"No hay slugs disponibles para '{root}'."); }
    private static string RemoveDiacritics(string text){var n=text.Normalize(NormalizationForm.FormD);var b=new StringBuilder();foreach(var c in n)if(CharUnicodeInfo.GetUnicodeCategory(c)!=UnicodeCategory.NonSpacingMark)b.Append(c);return b.ToString().Normalize(NormalizationForm.FormC);}
}
