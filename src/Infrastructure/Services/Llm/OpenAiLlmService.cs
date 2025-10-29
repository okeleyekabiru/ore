using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ore.Application.Abstractions.Llm;
using Ore.Application.Common.Models;
using Ore.Domain.ValueObjects;
using Ore.Infrastructure.Options;

namespace Ore.Infrastructure.Services.Llm;

public sealed class OpenAiLlmService : ILlmService
{
    private readonly OpenAiOptions _options;
    private readonly ILogger<OpenAiLlmService> _logger;

    public OpenAiLlmService(IOptions<OpenAiOptions> options, ILogger<OpenAiLlmService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task<GeneratedContentResult> GenerateContentAsync(Guid teamId, BrandVoiceProfile? voiceProfile, string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return Task.FromResult(new GeneratedContentResult(string.Empty, Array.Empty<string>(), string.Empty, string.Empty));
        }

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning("OpenAI API key is not configured. Falling back to synthesized response.");
        }

        var context = PromptContext.From(prompt);
        var caption = BuildCaption(context, voiceProfile);
        var hashtags = BuildHashtags(context, voiceProfile);
        var imageIdea = BuildImageIdea(context, voiceProfile);
        var rawPayload = BuildRawPayload(teamId, context, voiceProfile, hashtags);

        return Task.FromResult(new GeneratedContentResult(caption, hashtags, imageIdea, rawPayload));
    }

    private static string BuildCaption(PromptContext context, BrandVoiceProfile? profile)
    {
        var random = Random.Shared;

        var emojis = new[] { "✨", "🔥", "💡", "🌟", "🚀", "🎯", "📣", "🌱" };
        var hooks = new[]
        {
            "takes center stage",
            "is our spotlight today",
            "deserves the conversation",
            "is shaping the narrative",
            "gets the inside track"
        };
        var connectors = new[]
        {
            "Let’s translate it into moments that matter.",
            "Here’s how we bring it to life for real people.",
            "Translating strategy into scroll-stopping storytelling.",
            "We’re threading it through the brand story in style.",
            "Let’s turn insight into impact." 
        };
        var callsToAction = new[]
        {
            "Ready to make it happen?",
            "Tell us how this lands.",
            "Bookmark for the next campaign sprint.",
            "Slide into the brief and let’s run with it.",
            "Share the vibe with your crew." 
        };

        var emoji = emojis[random.Next(emojis.Length)];
        var hook = hooks[random.Next(hooks.Length)];
        var connector = connectors[random.Next(connectors.Length)];
        var cta = callsToAction[random.Next(callsToAction.Length)];

        var tone = !string.IsNullOrWhiteSpace(context.Tone)
            ? context.Tone.ToLowerInvariant()
            : profile?.Tone?.ToLowerInvariant();
        var audience = profile?.Audience;

        var openingTopic = Capitalize(context.Topic ?? "The big idea");

        var segments = new List<string>
        {
            $"{emoji} {openingTopic} {hook}"
        };

        if (!string.IsNullOrWhiteSpace(tone))
        {
            segments.Add($"We keep it {tone} and true to the brand voice.");
        }

        if (!string.IsNullOrWhiteSpace(context.Goal))
        {
            segments.Add($"Goal: {context.Goal}.");
        }

        if (!string.IsNullOrWhiteSpace(audience))
        {
            segments.Add($"Built for {audience}.");
        }

        segments.Add(connector);
        segments.Add(cta);

        return string.Join(' ', segments.Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
    }

    private static string[] BuildHashtags(PromptContext context, BrandVoiceProfile? profile)
    {
        var results = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void TryAdd(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            var sanitized = new string(value.Where(char.IsLetterOrDigit).ToArray());
            if (string.IsNullOrWhiteSpace(sanitized))
            {
                return;
            }

            var tag = "#" + sanitized.ToLowerInvariant();
            if (seen.Add(tag))
            {
                results.Add(tag);
            }
        }

        if (!string.IsNullOrWhiteSpace(context.Topic))
        {
            foreach (var word in context.Topic.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Take(3))
            {
                TryAdd(word);
            }
        }

        if (profile?.Keywords is not null)
        {
            foreach (var keyword in profile.Keywords.Take(4))
            {
                TryAdd(keyword);
            }
        }

        var defaults = new[]
        {
            "brandstory",
            "contentstrategy",
            "marketingmindset",
            "creativeops",
            "socialfirst",
            "campaigns"
        };

        foreach (var fallback in defaults)
        {
            if (results.Count >= 6)
            {
                break;
            }

            TryAdd(fallback);
        }

        return results.Take(6).ToArray();
    }

    private static string BuildImageIdea(PromptContext context, BrandVoiceProfile? profile)
    {
        var tone = profile?.Tone ?? context.Tone;
        var focus = !string.IsNullOrWhiteSpace(context.Topic) ? context.Topic : "the brand story";
        var audience = profile?.Audience;

        var fragments = new List<string>
        {
            $"Concept art highlighting {focus}"
        };

        if (!string.IsNullOrWhiteSpace(tone))
        {
            fragments.Add($"with a {tone.ToLowerInvariant()} mood");
        }
        else
        {
            fragments.Add("with confident, energetic lighting");
        }

        if (profile?.Keywords is { Count: > 0 } keywords)
        {
            fragments.Add($"accented by {string.Join(", ", keywords.Take(3))}");
        }

        if (!string.IsNullOrWhiteSpace(audience))
        {
            fragments.Add($"designed to resonate with {audience}");
        }

        return string.Join(" ", fragments) + ".";
    }

    private string BuildRawPayload(Guid teamId, PromptContext context, BrandVoiceProfile? profile, IReadOnlyCollection<string> hashtags)
    {
        var payload = new
        {
            model = string.IsNullOrWhiteSpace(_options.ApiKey) ? $"synthetic-{_options.Model}" : _options.Model,
            generatedAtUtc = DateTimeOffset.UtcNow,
            teamId,
            context.Platform,
            context.Topic,
            context.Tone,
            context.Goal,
            brandVoice = profile is null
                ? null
                : new
                {
                    profile.Voice,
                    profile.Tone,
                    profile.Audience,
                    Keywords = profile.Keywords
                },
            hashtags = hashtags.ToArray()
        };

        return JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private static string Capitalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.Trim());
    }

    private sealed record PromptContext(string Platform, string Topic, string Tone, string Goal)
    {
        public static PromptContext From(string prompt)
        {
            var platform = Extract(prompt, @"Create a\s+(?<value>[\w\s]+?)\s+post", "value");
            var topic = Extract(prompt, @"topic:\s*(?<value>[^\.\r\n]+)", "value");
            var tone = Extract(prompt, @"tone that feels\s+(?<value>[^\.\r\n]+)", "value");
            var goal = Extract(prompt, @"Goals:\s*(?<value>[^\r\n]+)", "value");

            return new PromptContext(platform, topic, tone, goal);
        }

        private static string Extract(string source, string pattern, string groupName)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return string.Empty;
            }

            var match = Regex.Match(source, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (!match.Success)
            {
                return string.Empty;
            }

            var group = match.Groups[groupName];
            return group.Success ? group.Value.Trim() : string.Empty;
        }
    }
}
