namespace Domain.Interfaces;

public interface IBudgetOperation
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public OperationType Type { get; set; }
    public double Sum { get; set; }
    public IBudgetItem? Item { get; set; }
}
