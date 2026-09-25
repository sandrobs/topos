using Microsoft.AspNetCore.Antiforgery;

namespace CrmIctm.Api.Endpoints;

public static class SegurancaEndpoints
{
    public static IEndpointRouteBuilder MapearSeguranca(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/seguranca/token-csrf", (HttpContext contexto, IAntiforgery antiforgery) =>
        {
            var tokens = antiforgery.GetAndStoreTokens(contexto);
            return Results.Ok(new { token = tokens.RequestToken });
        }).AllowAnonymous();

        return endpoints;
    }
}

