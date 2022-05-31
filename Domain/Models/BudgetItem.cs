namespace Domain.Models;

public class BudgetItem : IBudgetItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public OperationType Type { get; set; }
}
