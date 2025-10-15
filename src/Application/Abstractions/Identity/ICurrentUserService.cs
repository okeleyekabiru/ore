using System;

namespace Ore.Application.Abstractions.Identity;

public interface ICurrentUserService
{
    Guid GetUserId();
}
