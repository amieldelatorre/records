using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Common;

public class BaseEntity
{
    [Key]
    [Column(TypeName = "uuid")]
    public Guid Id { get; set; }

    [Required]
    [Column(TypeName = "timestamp with time zone")]
    public required DateTime DateCreated { get; set; }

    [Required]
    [Column(TypeName = "timestamp with time zone")]
    public required DateTime DateUpdated { get; set; }
}