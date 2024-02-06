namespace Commons.Models;

public class VersionObject
{
    public long Id { get; set; }
    public long FileId { get; set; }
    public string? Filename { get; set; }
    public string? Text { get; set; }
    public string? Tag { get; set; }
}
