using System.Linq;
using Ore.Domain.Common;

namespace Ore.Domain.ValueObjects;

public sealed class BrandVoiceProfile : ValueObject
{
    public string Voice { get; }
    public string Tone { get; }
    public string Audience { get; }
    public IReadOnlyCollection<string> Keywords { get; }

    private BrandVoiceProfile(string voice, string tone, string audience, IEnumerable<string> keywords)
    {
        Voice = voice;
        Tone = tone;
        Audience = audience;
        Keywords = keywords.Select(k => k.Trim()).Where(k => !string.IsNullOrWhiteSpace(k)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    public static BrandVoiceProfile Create(string voice, string tone, string audience, IEnumerable<string> keywords)
    {
        return new BrandVoiceProfile(
            voice.Trim(),
            tone.Trim(),
            audience.Trim(),
            keywords ?? Array.Empty<string>());
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Voice;
        yield return Tone;
        yield return Audience;
        foreach (var keyword in Keywords)
        {
            yield return keyword;
        }
    }
}
