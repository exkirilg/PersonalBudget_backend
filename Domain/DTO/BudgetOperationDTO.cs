using System.ComponentModel.DataAnnotations;

namespace Domain.DTO;

public class BudgetOperationDTO
{
    [Required(ErrorMessage = "Date is required")]
    public DateTime Date { get; set; }

    [Required(ErrorMessage = "Budget item id is required")]
    public int ItemId { get; set; }

    public double Sum { get; set; }
}
