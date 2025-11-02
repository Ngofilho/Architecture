namespace Common;

public class OrderMessage : Message
{
    public IEnumerable<OrderDetails>? OrderDetails { get; set; }
    public IEnumerable<PersonMessage>? Customers { get; set; }
}
public class OrderDetails
{
    public int ItemId { get; set; }
    public decimal Quantity { get; set; }
    public int ClientId { get; set; }
}