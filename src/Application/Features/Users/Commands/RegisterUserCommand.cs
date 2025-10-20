using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ore.Application.Abstractions.Identity;
using Ore.Application.Abstractions.Infrastructure;
using Ore.Application.Abstractions.Persistence;
using Ore.Application.Common.Models;
using Ore.Domain.Entities;
using Ore.Domain.Enums;
using Ore.Domain.ValueObjects;

namespace Ore.Application.Features.Users.Commands;

public sealed record BrandSurveyOnboardingInput(
    string Voice,
    string Tone,
    string Audience,
    string Goals,
    string Competitors,
    IEnumerable<string> Keywords);

public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    RoleType Role,
    string? TeamName,
    bool IsIndividual,
    BrandSurveyOnboardingInput BrandSurvey) : IRequest<Result<Guid>>;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IIdentityService _identityService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RegisterUserCommandHandler(
        IApplicationDbContext dbContext,
        IIdentityService identityService,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _identityService = identityService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var existing = await _dbContext.Users
            .Where(u => u.Email == normalizedEmail)
            .Select(u => u.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing != default)
        {
            return Result<Guid>.Failure("User already exists");
        }

        Team? team = null;

        if (request.IsIndividual)
        {
            team = new Team($"{request.FirstName} {request.LastName}".Trim());
            _dbContext.Teams.Add(team);
        }
        else if (!string.IsNullOrWhiteSpace(request.TeamName))
        {
            team = await _dbContext.Teams
                .FirstOrDefaultAsync(t => t.Name == request.TeamName, cancellationToken);

            if (team is null)
            {
                team = new Team(request.TeamName);
                _dbContext.Teams.Add(team);
            }
        }

        if (team is not null && request.BrandSurvey is not null)
        {
            var profile = BrandVoiceProfile.Create(
                request.BrandSurvey.Voice,
                request.BrandSurvey.Tone,
                request.BrandSurvey.Audience,
                request.BrandSurvey.Goals,
                request.BrandSurvey.Competitors,
                request.BrandSurvey.Keywords ?? Array.Empty<string>());

            team.SetBrandVoice(profile);
        }

        var identityId = await _identityService.CreateUserAsync(
            normalizedEmail,
            request.Password,
            new[] { request.Role.ToString() },
            cancellationToken);

        var user = new User(identityId, normalizedEmail, request.FirstName, request.LastName, request.Role);

        if (team is not null)
        {
            user.AssignTeam(team);
            team.AddMember(user);
        }

        user.CreatedOnUtc = _dateTimeProvider.UtcNow;
        user.CreatedBy = identityId.ToString();

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(user.Id);
    }
}
