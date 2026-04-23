using CyShop.ServiceDefaults.Models;
using Microsoft.AspNetCore.Http;

namespace CyShop.ServiceDefaults;

public class CurrentUserMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context, CurrentUser currentUser)
    {
        var endpoint = context.GetEndpoint();

        var hasAuthMetadata = endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.IAuthorizeData>() is not null;
        if (!hasAuthMetadata)
        {
            return next(context);
        }

        var user = context.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            return next(context);
        }

        var externalId = user.ResolveExternalId(context);
        if (externalId is null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        currentUser.Set(externalId.Value);
        return next(context);
    }
}
