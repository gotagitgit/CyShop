namespace S3Client.Clients;

public interface IS3StorageClient
{
    Task UploadAsync(string bucketName, string key, Stream stream, string contentType, CancellationToken ct = default);
    Task<Stream> DownloadAsync(string bucketName, string key, CancellationToken ct = default);
    Task DeleteAsync(string bucketName, string key, CancellationToken ct = default);
    Task<bool> ExistsAsync(string bucketName, string key, CancellationToken ct = default);
    Task CreateBucketIfNotExistsAsync(string bucketName, CancellationToken ct = default);
    Task<bool> BucketHasObjectsAsync(string bucketName, CancellationToken ct = default);
}
