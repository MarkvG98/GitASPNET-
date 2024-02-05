namespace Commons.Models;

public class FileObject
{
    public long Id { get; set; }
    public string? Filename { get; set; }
    public string? Text { get; set; }
    public bool Locked { get; set; }
}
