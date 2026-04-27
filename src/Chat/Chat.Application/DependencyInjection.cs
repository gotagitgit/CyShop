namespace Chat.Application;

using Chat.Application.Interfaces;
using Chat.Application.Services;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IChatService, ChatService>();
        return services;
    }
}
