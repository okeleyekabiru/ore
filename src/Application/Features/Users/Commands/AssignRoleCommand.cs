using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ore.Application.Abstractions.Identity;
using Ore.Application.Abstractions.Persistence;
using Ore.Application.Common.Models;
using Ore.Domain.Entities;
using Ore.Domain.Enums;

namespace Ore.Application.Features.Users.Commands;

public sealed record AssignRoleCommand(Guid UserId, RoleType Role) : IRequest<Result<Guid>>;

public sealed class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IIdentityService _identityService;

    public AssignRoleCommandHandler(IApplicationDbContext dbContext, IIdentityService identityService)
    {
        _dbContext = dbContext;
        _identityService = identityService;
    }

    public async Task<Result<Guid>> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user is null)
        {
            return Result<Guid>.Failure("User not found.");
        }

        await _identityService.AssignRoleAsync(user.Id, request.Role.ToString(), cancellationToken);
        user.ChangeRole(request.Role);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(user.Id);
    }
}
