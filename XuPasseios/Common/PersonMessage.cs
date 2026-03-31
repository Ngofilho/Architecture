namespace Common;

public abstract class PersonMessage
{
    public Guid ClientId { get; set; }
}

public class Person : PersonMessage
{
}