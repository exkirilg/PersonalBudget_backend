using System.ComponentModel.DataAnnotations;

namespace Domain.DTO;

public class ChangePasswordDTO : IDTO
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "New password is required")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "New password confirmation is required")]
    [Compare(nameof(NewPassword), ErrorMessage = "New password confirmation does not match")]
    public string NewPasswordConfirmation { get; set; } = string.Empty;
}
