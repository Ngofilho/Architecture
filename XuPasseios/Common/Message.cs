namespace Common;

public abstract class Message
{
    public Guid MessageId { get; set; }
    public string MessageVersion { get; set; } = "1.0";
    public string? Origin { get; set; }
    public string? Destiny { get; set; }
    public DateTime DateOfSending { get; set; } = DateTime.UtcNow;
}
