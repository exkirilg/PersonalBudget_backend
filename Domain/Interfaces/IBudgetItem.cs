namespace Domain.Interfaces;

public interface IBudgetItem
{
    public int Id { get; set; }
    public string Name { get; set; }
    public OperationType Type { get; set; }
}
