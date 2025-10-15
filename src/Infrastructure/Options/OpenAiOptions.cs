namespace Ore.Infrastructure.Options;

public sealed class OpenAiOptions
{
    public const string SectionName = "OpenAi";

    public string ApiKey { get; init; } = string.Empty;
    public string Model { get; init; } = "gpt-4.1";
}
