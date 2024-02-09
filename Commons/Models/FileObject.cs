namespace Commons.Models;

public class FileObject
{
    public long Id { get; set; }
    public List<long> VersionIds { get; set; }
    public bool Locked { get; set; }
}
