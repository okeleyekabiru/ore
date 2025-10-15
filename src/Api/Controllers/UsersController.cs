using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ore.Api.Common;
using Ore.Application.Abstractions.Identity;
using Ore.Application.Features.Users.Queries;

namespace Ore.Api.Controllers;

[Authorize]
public sealed class UsersController : ApiControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    public UsersController(IMediator mediator, ICurrentUserService currentUserService) : base(mediator)
    {
        _currentUserService = currentUserService;
    }

    [HttpGet("detail")]
    public async Task<IActionResult> GetDetail(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();
        var result = await Mediator.Send(new GetUserProfileQuery(userId), cancellationToken);
        return FromResult(result);
    }

    [HttpGet("{userId:guid}")]
    [Authorize(Roles = nameof(Domain.Enums.RoleType.Admin))]
    public async Task<IActionResult> GetById(Guid userId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetUserProfileQuery(userId), cancellationToken);
        return FromResult(result);
    }
}
