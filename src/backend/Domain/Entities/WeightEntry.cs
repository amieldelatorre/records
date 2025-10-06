namespace Domain.Entities;

public class WeightEntry : BaseEntity
{
    // Value is in kilograms
    public required decimal Value { get; set; }
    public string? Comment { get; set; }
    
    // yyyy-mm-dd format
    public required DateOnly EntryDate { get; set; }
    
    public required Guid UserId { get; set; }
    public User OwningUser { get; set; }
}