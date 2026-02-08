using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity_Auth_MicroService.Shared.IdentityDTO
{
    public record RegisterDTO
        (
        [Required]
        string DisplayName,
        [Required]
        [EmailAddress]
        string Email,
        [Required]
        string Password,
        [Required]
        [Phone]
        string PhoneNumber,
        string? role = "User"
        );
}
