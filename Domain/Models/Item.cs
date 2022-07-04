namespace Domain.Models;

public class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public OperationType Type { get; set; }
}
