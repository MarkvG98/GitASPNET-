namespace Commons.Models;

public class VersionObject
{
    public long Id { get; set; }
    public long[] FileIds { get; set; }
    public string? Tag { get; set; }
}
