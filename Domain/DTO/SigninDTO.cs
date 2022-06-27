using System.ComponentModel.DataAnnotations;

namespace Domain.DTO;

public class SigninDTO
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}
