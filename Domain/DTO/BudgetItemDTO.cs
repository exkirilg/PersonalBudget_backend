using System.ComponentModel.DataAnnotations;

namespace Domain.DTO;

public class BudgetItemDTO
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
