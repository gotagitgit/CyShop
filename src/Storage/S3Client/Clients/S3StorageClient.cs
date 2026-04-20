using Amazon.S3;
using Amazon.S3.Model;

namespace S3Client.Clients;

internal sealed class S3StorageClient(IAmazonS3 s3Client) : IS3StorageClient
{
    public async Task UploadAsync(string bucketName, string key, Stream stream, string contentType, CancellationToken ct = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = key,
            InputStream = stream,
            ContentType = contentType
        };

        await s3Client.PutObjectAsync(request, ct);
    }

    public async Task<Stream> DownloadAsync(string bucketName, string key, CancellationToken ct = default)
    {
        var response = await s3Client.GetObjectAsync(bucketName, key, ct);
        return response.ResponseStream;
    }

    public async Task DeleteAsync(string bucketName, string key, CancellationToken ct = default)
    {
        await s3Client.DeleteObjectAsync(bucketName, key, ct);
    }

    public async Task<bool> ExistsAsync(string bucketName, string key, CancellationToken ct = default)
    {
        try
        {
            await s3Client.GetObjectMetadataAsync(bucketName, key, ct);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task CreateBucketIfNotExistsAsync(string bucketName, CancellationToken ct = default)
    {
        try
        {
            await s3Client.PutBucketAsync(bucketName, ct);
        }
        catch (AmazonS3Exception ex) when (
            ex.ErrorCode == "BucketAlreadyOwnedByYou" ||
            ex.ErrorCode == "BucketAlreadyExists")
        {
            // Bucket already exists, nothing to do
        }
    }

    public async Task<bool> BucketHasObjectsAsync(string bucketName, CancellationToken ct = default)
    {
        var request = new ListObjectsV2Request
        {
            BucketName = bucketName,
            MaxKeys = 1
        };

        var response = await s3Client.ListObjectsV2Async(request, ct);
        return response.S3Objects?.Count > 0;
    }

    public async Task DeleteAllObjectsAsync(string bucketName, CancellationToken ct = default)
    {
        var request = new ListObjectsV2Request { BucketName = bucketName };
        ListObjectsV2Response response;

        do
        {
            response = await s3Client.ListObjectsV2Async(request, ct);

            if (response.S3Objects is { Count: > 0 })
            {
                var deleteRequest = new DeleteObjectsRequest
                {
                    BucketName = bucketName,
                    Objects = response.S3Objects.Select(o => new KeyVersion { Key = o.Key }).ToList()
                };

                await s3Client.DeleteObjectsAsync(deleteRequest, ct);
            }

            request.ContinuationToken = response.NextContinuationToken;
        }
        while (response.IsTruncated == true);
    }
}
