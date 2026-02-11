namespace DeadlockDemo.Models;

public class Inventory
{
    public int ProductId { get; set; }
    public int AvailableQty { get; set; }
    public int ReservedQty { get; set; }
}


