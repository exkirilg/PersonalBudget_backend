using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.DTO;

public class OperationDTO : IDTO
{
    [Required(ErrorMessage = "Date is required")]
    public DateTime Date { get; set; }

    [Required(ErrorMessage = "Item id is required")]
    public int ItemId { get; set; }

    [JsonIgnore]
    public Item? Item { get; set; } = default;

    public double Sum { get; set; }
}
