namespace Application.Common;

public class BaseEntityResponse
{
    public Guid Id { get; set; }
    public required DateTime DateCreated { get; set; }
    public required DateTime DateUpdated { get; set; }
}