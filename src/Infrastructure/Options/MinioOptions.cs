namespace Ore.Infrastructure.Options;

public sealed class MinioOptions
{
    public const string SectionName = "Minio";

    public string Endpoint { get; init; } = string.Empty;
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public string BucketName { get; init; } = "media";
    public bool UseSsl { get; init; } = true;
    public string? Region { get; init; }
}
