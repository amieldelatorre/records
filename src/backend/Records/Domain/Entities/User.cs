using Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class User : BaseEntity
{
    [Required]
    [Column(TypeName = "text")]
    public required string FirstName { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public required string LastName { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public required string Email { get; set; }
    
    [Required]
    [Column(TypeName = "text")]
    public required string PasswordHash { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public required string PasswordSalt { get; set; }
}