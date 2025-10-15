namespace Ore.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedOnUtc { get; set; }
    public string? ModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
}
