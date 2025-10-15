using System;
using System.Threading;
using System.Threading.Tasks;
using Ore.Domain.ValueObjects;

namespace Ore.Application.Abstractions.Llm;

public interface ILlmService
{
    Task<string> GeneratePostAsync(Guid teamId, BrandVoiceProfile? voiceProfile, string prompt, CancellationToken cancellationToken = default);
}
