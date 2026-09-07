namespace Dhole.Content.Application.Abstractions.Slugs;
public interface ISlugGenerator
{
    string Generate(string value);
    Task<string> GenerateUniqueAsync(string value,Func<string,CancellationToken,Task<bool>> existsAsync,CancellationToken cancellationToken=default);
}
