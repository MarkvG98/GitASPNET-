namespace GitProjektWHS
{
    public class PushRepository
    {
        //TODO: Implementieren ohne auf GitProjektWHS zuzugreifen
        //public void StartPush(string pisourceDirectory, VersionsContext dbContext)
        //{
        //    CreateAndUpdate_Caller(ReadAllFiles(pisourceDirectory), dbContext);
        //}

        private List<VersionsObjekt> ReadAllFiles(string pisourceDirectory)
        {

            DirectoryInfo dirInfo = new DirectoryInfo(pisourceDirectory);

            var Files = dirInfo.GetFiles("*.txt");

            List<VersionsObjekt> versions_List = new List<VersionsObjekt>();

            foreach (FileInfo file in Files)
            {
                var Inhalt = File.ReadAllText(file.FullName);
                var indexLine = File.ReadLines(file.FullName).First().Split(';');
                var VersionsDateiID = int.Parse(indexLine.First());
                var VersionsObjektID = int.Parse(indexLine.Last());

                versions_List.Add(
                    new VersionsObjekt
                    {
                        ID = VersionsObjektID,
                        DateiID = VersionsDateiID,
                        Filename = file.Name,
                        Inhalt = Inhalt,
                        Version = VersionsObjektID++
                    });
            }
            return versions_List;
        }

        //TODO: Implementieren ohne auf GitProjektWHS zuzugreifen
        //private void CreateAndUpdate_Caller(List<VersionsObjekt> piVersionsObjektList, VersionsContext dbContext)
        //{
        //    var _versionsController = new VersionsController(dbContext);
        //    List<VersionsObjekt> update_List = new List<VersionsObjekt>();
        //    List<VersionsObjekt> create_List = new List<VersionsObjekt>();
        //    foreach (var versionsObjekt in piVersionsObjektList)
        //    {
        //        var Objekt = (VersionsObjekt)_versionsController.GetVersionsObjekt(versionsObjekt.ID);

        //        if (Objekt != null)
        //        {
        //            _versionsController.UpdateVersionsObjekt(versionsObjekt);
        //        }
        //        else
        //        {
        //            _versionsController.CreateVersionsDatei(new VersionsDatei { Id = versionsObjekt.DateiID , Lock = false });// hier ein Get HighestID, Wenn keine vorhanden und autoEintrag
        //            _versionsController.CreateVersionsObjekt(versionsObjekt);
        //        }                
        //    }
        //}   
    }
}
