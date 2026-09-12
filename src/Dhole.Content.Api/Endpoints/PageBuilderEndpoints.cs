using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Content.Api.Authorization;
using Dhole.Content.Api.Extensions;
using Dhole.Content.Application.PageBuilder.ApplyPageBuilderOperation;
using Dhole.Content.Application.PageBuilder.GetPageBuilder;
using Dhole.Content.Contracts.PageBuilder;
using Dhole.Content.Domain.PageBuilder;

namespace Dhole.Content.Api.Endpoints;

public static class PageBuilderEndpoints
{
    public static IEndpointRouteBuilder MapPageBuilderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/content/page-builder")
            .WithTags("Page Builder")
            .RequireAuthorization();

        group.MapGet(
                "/block-types",
                () => EndpointResults.Ok(PageBuilderBlockTypes.All
                    .Select(type => new PageBuilderBlockTypeDto(type, GetLabel(type)))
                    .ToArray()))
            .RequireScope(ContentScopeNames.View);

        group.MapGet(
                "/{contentId:guid}",
                async (
                    Guid contentId,
                    IQueryDispatcher dispatcher,
                    HttpContext context,
                    CancellationToken cancellationToken
                ) => EndpointResults.FromResult(
                    await dispatcher.DispatchAsync(new GetPageBuilderQuery(contentId), cancellationToken),
                    context))
            .RequireScope(ContentScopeNames.View);

        group.MapPost(
                "/{contentId:guid}/operations",
                async (
                    Guid contentId,
                    PageBuilderOperationRequest request,
                    ICommandDispatcher dispatcher,
                    HttpContext context,
                    CancellationToken cancellationToken
                ) => EndpointResults.FromResult(
                    await dispatcher.DispatchAsync(
                        new ApplyPageBuilderOperationCommand(
                            contentId,
                            request.Operation,
                            request.BlockId,
                            request.BlockType,
                            request.TargetIndex,
                            request.IsVisible,
                            request.DataJson,
                            context.GetCurrentUserId()),
                        cancellationToken),
                    context))
            .RequireScope(ContentScopeNames.PagesEdit);

        return app;
    }

    private static string GetLabel(string type) => type switch
    {
        PageBuilderBlockTypes.Hero => "Hero",
        PageBuilderBlockTypes.RichText => "Texto enriquecido",
        PageBuilderBlockTypes.Image => "Imagen",
        PageBuilderBlockTypes.Video => "Video",
        PageBuilderBlockTypes.Gallery => "Galería",
        PageBuilderBlockTypes.CTA => "Llamado a la acción",
        PageBuilderBlockTypes.ServicesGrid => "Servicios",
        PageBuilderBlockTypes.NewsGrid => "Noticias",
        PageBuilderBlockTypes.FAQ => "Preguntas frecuentes",
        PageBuilderBlockTypes.Testimonials => "Testimonios",
        PageBuilderBlockTypes.Logos => "Logos",
        PageBuilderBlockTypes.Stats => "Estadísticas",
        PageBuilderBlockTypes.Team => "Equipo",
        PageBuilderBlockTypes.Banner => "Banner",
        PageBuilderBlockTypes.Form => "Formulario",
        PageBuilderBlockTypes.MeetingForm => "Formulario de reunión",
        _ => type
    };
}
