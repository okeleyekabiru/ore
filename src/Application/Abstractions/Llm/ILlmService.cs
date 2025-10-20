using System;
using System.Threading;
using System.Threading.Tasks;
using Ore.Application.Common.Models;
using Ore.Domain.ValueObjects;

namespace Ore.Application.Abstractions.Llm;

public interface ILlmService
{
    Task<GeneratedContentResult> GenerateContentAsync(Guid teamId, BrandVoiceProfile? voiceProfile, string prompt, CancellationToken cancellationToken = default);
}
