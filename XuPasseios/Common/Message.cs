namespace Common;

public abstract class Message
{
    public Guid Id { get; set; }
    public string Version { get; set; } = "1.0";
    public string? Origin { get; set; }
    public string? Destiny { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
}
