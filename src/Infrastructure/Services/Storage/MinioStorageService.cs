using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Ore.Application.Abstractions.Storage;
using Ore.Infrastructure.Options;

namespace Ore.Infrastructure.Services.Storage;

public sealed class MinioStorageService : IMediaStorageService
{
    private readonly IMinioClient _client;
    private readonly MinioOptions _options;
    private readonly ILogger<MinioStorageService> _logger;

    public MinioStorageService(IOptions<MinioOptions> options, ILogger<MinioStorageService> logger)
    {
        _options = options.Value;
        _logger = logger;

        IMinioClient client = new MinioClient()
            .WithEndpoint(_options.Endpoint)
            .WithCredentials(_options.AccessKey, _options.SecretKey)
            .WithSSL(_options.UseSsl);

        if (!string.IsNullOrWhiteSpace(_options.Region))
        {
            client = client.WithRegion(_options.Region);
        }

        _client = client;
    }

    public async Task<string> UploadAsync(Stream content, string contentType, string fileName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.BucketName))
        {
            throw new InvalidOperationException("MinIO bucket name is not configured.");
        }

        await EnsureBucketExistsAsync(cancellationToken).ConfigureAwait(false);

        var objectName = $"{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid()}-{fileName}";

        await using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
        buffer.Position = 0;

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectName)
            .WithStreamData(buffer)
            .WithObjectSize(buffer.Length)
            .WithContentType(contentType);

        await _client.PutObjectAsync(putObjectArgs, cancellationToken).ConfigureAwait(false);

        var scheme = _options.UseSsl ? "https" : "http";
        var objectUrl = $"{scheme}://{_options.Endpoint}/{_options.BucketName}/{objectName}";

        _logger.LogInformation("Uploaded media file {FileName} to bucket {Bucket}.", fileName, _options.BucketName);
        return objectUrl;
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        var bucketArgs = new BucketExistsArgs().WithBucket(_options.BucketName);
        var exists = await _client.BucketExistsAsync(bucketArgs, cancellationToken).ConfigureAwait(false);
        if (!exists)
        {
            await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(_options.BucketName), cancellationToken).ConfigureAwait(false);
        }
    }
}
