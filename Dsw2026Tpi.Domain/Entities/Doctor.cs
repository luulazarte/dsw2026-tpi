namespace Dsw2026Tpi.Domain.Entities;

public class Doctor: EntityBase
{
    public string Name { get; private set; }
    public string LicenseNumber { get; private set; }
    public bool IsActive { get; private set; }
    public Guid SpecialityId { get; private set; }
    public Speciality Speciality { get; private set; }  
    public ICollection<AvailabilityRule> AvailabilityRules { get; private set; } = new List<AvailabilityRule>();

    #region Constructor for EF
#pragma warning disable CS8618
    private Doctor()
    {
    }
#pragma warning restore CS8618
    #endregion

    public Doctor(string name, string licenseNumber, Guid specialityId, Guid? id = null) : base(id)
    {
        Name = name;
        LicenseNumber = licenseNumber;
        SpecialityId = specialityId;
        IsActive = true;
    }
  
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.Now;
    }
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.Now;
    }
}
