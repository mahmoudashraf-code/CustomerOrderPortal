namespace Backend.Models;

public class OrderDeletion
{
    public int CustomerId { get; set; }
    public int OrderId { get; set; }
    public DateTime OrderCreationDate { get; set; }
    public DateTime DeletionDate { get; set; }
}

public class CustomerBanStatus
{
    public int CustomerId { get; set; }
    public DateTime BanStartTime { get; set; }
    public DateTime BanEndTime { get; set; }
    public bool IsCurrentlyBanned => DateTime.Now < BanEndTime;
}
