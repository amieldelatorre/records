namespace Domain.Entities;

public class BaseEntity
{
    public Guid Id { get; init; }
    public required DateTime DateCreated { get; set; }
    public required DateTime DateUpdated { get; set; }
}