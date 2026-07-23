namespace Dsw2026Tpi.Domain.Entities;

public abstract class EntityBase
{
    public Guid Id { get; protected set; }
    public bool Deleted { get; set; } = false;  
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    protected EntityBase(Guid? id = null)
    {
        Id = id ?? Guid.NewGuid();
    }
}
