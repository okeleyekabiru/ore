using System;
using System.Collections.Generic;
using System.Linq;
using Ore.Domain.Common;

namespace Ore.Domain.ValueObjects;

public sealed class BrandVoiceProfile : ValueObject
{
    public string Voice { get; private set; } = string.Empty;
    public string Tone { get; private set; } = string.Empty;
    public string Audience { get; private set; } = string.Empty;
    public IReadOnlyCollection<string> Keywords { get; private set; } = Array.Empty<string>();

    private BrandVoiceProfile()
    {
    }

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
