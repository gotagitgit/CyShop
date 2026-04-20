using Cyshop.Common.Services;
using S3Client.Clients;

namespace Storage.Infrastructure.Services;

internal sealed class StorageService(IS3StorageClient s3Client) : IStorageService
{
    public Task UploadAsync(string bucketName, string key, Stream stream, string contentType, CancellationToken ct = default)
        => s3Client.UploadAsync(bucketName, key, stream, contentType, ct);

    public Task<Stream> DownloadAsync(string bucketName, string key, CancellationToken ct = default)
        => s3Client.DownloadAsync(bucketName, key, ct);

    public Task DeleteAsync(string bucketName, string key, CancellationToken ct = default)
        => s3Client.DeleteAsync(bucketName, key, ct);

    public Task<bool> ExistsAsync(string bucketName, string key, CancellationToken ct = default)
        => s3Client.ExistsAsync(bucketName, key, ct);

    public Task CreateBucketIfNotExistsAsync(string bucketName, CancellationToken ct = default)
        => s3Client.CreateBucketIfNotExistsAsync(bucketName, ct);

    public Task<bool> BucketHasObjectsAsync(string bucketName, CancellationToken ct = default)
        => s3Client.BucketHasObjectsAsync(bucketName, ct);

    public Task DeleteAllObjectsAsync(string bucketName, CancellationToken ct = default)
        => s3Client.DeleteAllObjectsAsync(bucketName, ct);
}
