namespace Common;

public class OrderMessage : Message
{
    public IEnumerable<OrderDetails>? OrderDetails { get; set; }
    public IEnumerable<Person>? Customers { get; set; }
}
public class OrderDetails
{
    public Guid ItemId { get; set; }
    public decimal Quantity { get; set; }
    public Guid ClientId { get; set; }
    public decimal Price { get; set; }
}