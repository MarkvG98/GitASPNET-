using Commons.Models;

namespace GitProjektWHS
{
    public class PushRepository
    {
        //TODO: Implementieren ohne auf GitProjektWHS zuzugreifen
        //public void StartPush(string pisourceDirectory, VersionsContext dbContext)
        //{
        //    CreateAndUpdate_Caller(ReadAllFiles(pisourceDirectory), dbContext);
        //}

        private List<VersionObject> ReadAllFiles(string pisourceDirectory)
        {

            DirectoryInfo dirInfo = new DirectoryInfo(pisourceDirectory);

            var Files = dirInfo.GetFiles("*.txt");
            List<VersionObject> versions_List = new List<VersionObject>();

            foreach (FileInfo file in Files)
            {
                var Inhalt = File.ReadAllText(file.FullName);
                var indexLine = File.ReadLines(file.FullName).First().Split(';');
                var VersionsDateiID = int.Parse(indexLine.First());
                var VersionObjectID = int.Parse(indexLine.Last());
                versions_List.Add(
                    new VersionObject
                    {
                        Id = VersionObjectID,
                        FileId = VersionsDateiID,
                        Filename = file.Name,
                        Text = Inhalt,
                    });
            }
            return versions_List;
        }

        //TODO: Implementieren ohne auf GitProjektWHS zuzugreifen
        //private void CreateAndUpdate_Caller(List<VersionObject> piVersionObjectList, VersionsContext dbContext)
        //{
        //    var _versionsController = new VersionsController(dbContext);
        //    List<VersionObject> update_List = new List<VersionObject>();
        //    List<VersionObject> create_List = new List<VersionObject>();
        //    foreach (var VersionObject in piVersionObjectList)
        //    {
        //        var Objekt = (VersionObject)_versionsController.GetVersionObject(VersionObject.ID);

        //        if (Objekt != null)
        //        {
        //            _versionsController.UpdateVersionObject(VersionObject);
        //        }
        //        else
        //        {
        //            _versionsController.CreateVersionsDatei(new VersionsDatei { Id = VersionObject.DateiID, Lock = false });// hier ein Get HighestID, Wenn keine vorhanden und autoEintrag
        //            _versionsController.CreateVersionObject(VersionObject);
        //        }
        //    }
        //}
    }
}