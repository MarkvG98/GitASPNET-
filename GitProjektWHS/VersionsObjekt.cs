namespace GitProjektWHS
{
    public class VersionsObjekt
    {
        public int ID { get; set; }
        public int DateiID { get; set; }
        public int Version { get; set; }
        public string? Tag { get; set; }
        public string? Inhalt { get; set; }
        public string? Filename { get; set; }
    }
}
