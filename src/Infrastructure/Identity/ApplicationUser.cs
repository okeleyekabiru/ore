using Microsoft.AspNetCore.Identity;

namespace Ore.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public Guid? TeamId { get; set; }
}
