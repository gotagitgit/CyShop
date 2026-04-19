using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using S3Client;
using Storage.Infrastructure.Services;

namespace Storage.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddStorageInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddS3StorageClient(configuration);
        services.AddSingleton<IStorageService, StorageService>();
        return services;
    }
}
