namespace Domain.Entities;

public class WeightEntry : BaseEntity
{
    public required double Value { get; set; }
    public string? Comment { get; set; }
    public required DateOnly EntryDate { get; set; }
    
    public required Guid UserId { get; set; }
    public User OwningUser { get; set; }
}