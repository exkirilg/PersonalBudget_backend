namespace Domain.Models;

public class BudgetOperation : IBudgetOperation
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public OperationType Type { get; set; }
    public double Sum { get; set; }
    public IBudgetItem? Item { get; set; }
    public string AuthorId { get; set; } = string.Empty;
}
