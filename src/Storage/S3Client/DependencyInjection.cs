using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using S3Client.Clients;

namespace S3Client;

public static class DependencyInjection
{
    public static IServiceCollection AddS3StorageClient(
        this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("Storage");

        services.AddSingleton<IAmazonS3>(_ =>
        {
            var config = new AmazonS3Config
            {
                ServiceURL = section["ServiceUrl"],
                ForcePathStyle = bool.Parse(section["ForcePathStyle"] ?? "true")
            };
            var credentials = new BasicAWSCredentials(
                section["AccessKey"], section["SecretKey"]);
            return new AmazonS3Client(credentials, config);
        });

        services.AddSingleton<IS3StorageClient, S3StorageClient>();

        return services;
    }
}
