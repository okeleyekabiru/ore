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
    public string Goals { get; private set; } = string.Empty;
    public string Competitors { get; private set; } = string.Empty;
    public IReadOnlyCollection<string> Keywords { get; private set; } = Array.Empty<string>();

    private BrandVoiceProfile()
    {
    }

    private BrandVoiceProfile(string voice, string tone, string audience, string goals, string competitors, IEnumerable<string> keywords)
    {
        Voice = voice;
        Tone = tone;
        Audience = audience;
        Goals = goals;
        Competitors = competitors;
        Keywords = keywords.Select(k => k.Trim()).Where(k => !string.IsNullOrWhiteSpace(k)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    public static BrandVoiceProfile Create(string voice, string tone, string audience, string goals, string competitors, IEnumerable<string> keywords)
    {
        return new BrandVoiceProfile(
            voice.Trim(),
            tone.Trim(),
            audience.Trim(),
            goals.Trim(),
            competitors.Trim(),
            keywords ?? Array.Empty<string>());
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Voice;
        yield return Tone;
        yield return Audience;
        yield return Goals;
        yield return Competitors;
        foreach (var keyword in Keywords)
        {
            yield return keyword;
        }
    }
}
