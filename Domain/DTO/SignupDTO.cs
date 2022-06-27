using System.ComponentModel.DataAnnotations;

namespace Domain.DTO;

public class SignupDTO
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    public string Password { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Password confirmation is required")]
    [Compare(nameof(Password), ErrorMessage = "Password confirmation does not match")]
    public string PasswordConfirmation { get; set; } = string.Empty;
}
