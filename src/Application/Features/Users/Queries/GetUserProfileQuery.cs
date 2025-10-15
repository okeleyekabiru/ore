using MediatR;
using Microsoft.EntityFrameworkCore;
using Ore.Application.Abstractions.Persistence;
using Ore.Application.Common.Models;
using Ore.Domain.Enums;

namespace Ore.Application.Features.Users.Queries;

public sealed record UserProfileDto(Guid Id, string Email, string FullName, RoleType Role, Guid? TeamId, string? TeamName);

public sealed record GetUserProfileQuery(Guid UserId) : IRequest<Result<UserProfileDto>>;

public sealed class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, Result<UserProfileDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetUserProfileQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<UserProfileDto>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .Include(u => u.Team)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            return Result<UserProfileDto>.Failure("User not found");
        }

        var dto = new UserProfileDto(
            user.Id,
            user.Email,
            user.FullName,
            user.Role,
            user.TeamId,
            user.Team?.Name);

        return Result<UserProfileDto>.Success(dto);
    }
}
