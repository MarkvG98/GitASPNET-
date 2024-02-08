using Commons.Models;
using System.Net;
using System.Net.Http.Headers;

namespace Client
{
    class Program
    {
        static HttpClient client = new HttpClient();

        static async Task<Uri> CreateVersionAsync(VersionObject version)
        {
            version.Timestamp = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            HttpResponseMessage response = await client.PostAsJsonAsync(
                "api/Versions", version);
            response.EnsureSuccessStatusCode();

            // return URI of the created resource.
            return response.Headers.Location;
        }

        static async Task<Uri> CreateFileAsync(FileObject file)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync(
                "api/Files", file);
            response.EnsureSuccessStatusCode();

            // return URI of the created resource.
            return response.Headers.Location;
        }

        static async Task<VersionObject> GetVersionAsync(string path)
        {
            VersionObject version = null;
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                version = await response.Content.ReadAsAsync<VersionObject>();
            }
            return version;
        }

        static async Task<FileObject> GetFileAsync(string path)
        {
            FileObject file = null;
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                file = await response.Content.ReadAsAsync<FileObject>();
            }
            return file;
        }

        static async Task<FileObject[]> GetFilesAsync()
        {
            FileObject[] files = null;
            HttpResponseMessage response = await client.GetAsync("api/Files");
            if (response.IsSuccessStatusCode)
            {
                files = await response.Content.ReadAsAsync<FileObject[]>();
            }
            return files;
        }

        static async Task<VersionObject> UpdateVersionAsync(VersionObject version)
        {
            HttpResponseMessage response = await client.PutAsJsonAsync(
                $"api/Versions/{version.Id}", version);
            response.EnsureSuccessStatusCode();

            // Deserialize the updated product from the response body.
            version = await response.Content.ReadAsAsync<VersionObject>();
            return version;
        }

        static async Task<FileObject> UpdateFileAsync(FileObject file)
        {
            HttpResponseMessage response = await client.PutAsJsonAsync(
                $"api/Files/{file.Id}", file);
            response.EnsureSuccessStatusCode();

            // Deserialize the updated product from the response body.
            file = await response.Content.ReadAsAsync<FileObject>();
            return file;
        }

        static async Task<HttpStatusCode> DeleteFileAsync(long id)
        {
            HttpResponseMessage response = await client.DeleteAsync(
                $"api/Files/{id}");
            return response.StatusCode;
        }

        static void Main()
        {
            client.BaseAddress = new Uri("http://localhost:5049/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            Console.WriteLine("Hallo zum GitProjektWHS! Für eine Liste aller Befehle bitte 'help' eingeben.");

            Task.WaitAll(HandleUserInput());
        }

        private async static Task HandleUserInput()
        {
            var userInput = Console.ReadLine();

            switch (userInput)
            {
                case "savefile":
                    await SaveFile();
                    break;
                case "getfile":
                    await GetFile();
                    break;
                case "getfilewithlock":
                    await GetFileWithLock();
                    break;
                case "addfile":
                    await AddFile();
                    break;
                case "resetfile":
                    await ResetFile();
                    break;
                case "createtag":
                    await CreateTag();
                    break;
                case "help":
                    // Befehle auflisten
                    Console.WriteLine("Befehle:\n" +
                                      "  savefile         Speichern einer Datei in einer neuen Version\n" +
                                      "  getfile          Holen der neuesten Version einer Datei vom Server\n" +
                                      "  getfilewithlock  Holen der neuesten Version einer Datei mit Sperren vom Server\n" +
                                      "  addfile          Einfügen einer neuen Datei\n" +
                                      "  resetfile        Zurücksetzen einer Datei auf eine alte Version\n" +
                                      "  createtag        Kennzeichnen einer Version mit einem Tag\n" +
                                      "  help             Befehle auflisten");
                    break;
                default:
                    Console.WriteLine("Unbekannter Befehl. Für eine Liste aller Befehle bitte 'help' eingeben.");
                    break;
            }

            await HandleUserInput();
        }

        static async Task GetFile()
        {
            // Holen der neuesten Version einer Datei vom Server

            // Hole alle Dateien vom Server und überprüfe, ob es überhautp welche gibt
            var remoteFiles = await GetFilesAsync();
            if (remoteFiles.Length != 0)
            {
                Console.WriteLine("Welche Datei möchtest du herunterladen? Gib bitte die Datei-ID an.");

                // Gib alle Dateien, aktuelle Dateinamen und Versions-IDs aus
                foreach (var remoteFile in remoteFiles)
                {
                    var remoteVersion = await GetVersionAsync("api/Versions/" + remoteFile.VersionIds.Max());

                    Console.WriteLine("  Datei-ID: {0}, Dateiname: {1}, aktuelle Version: {2}, Stand: {3}", remoteFile.Id, remoteVersion.Filename, remoteVersion.Id, remoteVersion.Timestamp);
                }

                // Überprüfe, ob die eingegebene Datei-ID existiert
                var fileId = Console.ReadLine();
                if (fileId == null)
                {
                    Console.WriteLine("Keine ID eingegeben");
                    return;
                }
                var file = await GetFileAsync("api/Files/" + fileId);
                if (file == null)
                {
                    Console.WriteLine("Die angegebene Datei-ID existiert nicht.");
                    return;
                }

                // Hole neueste Remote-Version der Datei
                var version = await GetVersionAsync("api/Versions/" + file.VersionIds.Max());

                // Speicherort eingeben
                Console.WriteLine("In welchem Ordner soll die Datei gespeichert werden?");
                var filePath = Console.ReadLine();

                // Erstelle die Datei samt Inhalt
                try
                {
                    File.WriteAllText(filePath + "\\" + version.Filename, version.Text);
                }
                catch
                {
                    Console.WriteLine("Ungültiger Dateipfad");
                    return;
                }

                // Bestätigung
                Console.WriteLine("Die Datei {0} (Datei-ID: {1}) wurde in Version {2} heruntergeladen.", version.Filename, file.Id, version.Id);
            }
            else
            {
                // Hinweis
                Console.WriteLine("Es gibt noch keine Dateien, die du herunterladen könntest. Lege mit 'addfile' eine Neue an.");
            }
        }

        static async Task GetFileWithLock()
        {
            // Holen der neuesten Version einer Datei mit Sperren vom Server

            // Hole alle Dateien vom Server und überprüfe, ob es überhautp welche gibt
            var remoteFiles = await GetFilesAsync();
            if (remoteFiles.Length != 0)
            {
                Console.WriteLine("Welche Datei möchtest du herunterladen und sperren? Gib bitte die Datei-ID an.");

                // Gib alle Dateien, aktuelle Dateinamen und Versions-IDs aus
                foreach (var remoteFile in remoteFiles)
                {
                    var remoteVersion = await GetVersionAsync("api/Versions/" + remoteFile.VersionIds.Max());

                    Console.WriteLine("  Datei-ID: {0}, Dateiname: {1}, aktuelle Version: {2}, Stand: {3}", remoteFile.Id, remoteVersion.Filename, remoteVersion.Id, remoteVersion.Timestamp);
                }

                // Überprüfe, ob die eingegebene Datei-ID existiert
                var fileId = Console.ReadLine();
                if (fileId == null)
                {
                    Console.WriteLine("Keine ID eingegeben");
                    return;
                }
                var file = await GetFileAsync("api/Files/" + fileId);
                if (file == null)
                {
                    Console.WriteLine("Die angegebene Datei-ID existiert nicht.");
                    return;
                }

                // Sperren
                file.Locked = true;
                file = await UpdateFileAsync(file);

                // Hole neueste Remote-Version der Datei
                var version = await GetVersionAsync("api/Versions/" + file.VersionIds.Max());

                // Speicherort eingeben
                Console.WriteLine("In welchem Ordner soll die Datei gespeichert werden?");
                var filePath = Console.ReadLine();

                // Erstelle die Datei samt Inhalt
                try
                {
                    File.WriteAllText(filePath + "\\" + version.Filename, version.Text);
                }
                catch
                {
                    Console.WriteLine("Ungültiger Dateipfad");
                    return;
                }

                // Bestätigung
                Console.WriteLine("Die Datei {0} (Datei-ID: {1}) wurde in Version {2} heruntergeladen.", version.Filename, file.Id, version.Id);
            }
            else
            {
                // Hinweis
                Console.WriteLine("Es gibt noch keine Dateien, die du herunterladen könntest. Lege mit 'addfile' eine Neue an.");
            }
        }

        static async Task SaveFile()
        {
            // Speichern einer Datei in einer neuen Version

            // Hole alle Dateien vom Server und überprüfe, ob es überhautp welche gibt
            var remoteFiles = await GetFilesAsync();
            if (remoteFiles.Length != 0)
            {
                Console.WriteLine("Für welche Datei möchtest du eine neue Version hochladen? Gib bitte die Datei-ID an.");

                // Gib alle Dateien, aktuelle Dateinamen und Versions-IDs aus
                foreach (var remoteFile in remoteFiles)
                {
                    var remoteVersion = await GetVersionAsync("api/Versions/" + remoteFile.VersionIds.Max());

                    Console.WriteLine("  Datei-ID: {0}, Dateiname: {1}, aktuelle Version: {2}, Stand: {3}", remoteFile.Id, remoteVersion.Filename, remoteVersion.Id, remoteVersion.Timestamp);
                }

                // TODO: Überprüfen, ob Datei gesperrt ist und ob man eine neue Version anlegen darf!
                // Überprüfe, ob die eingegebene Datei-ID existiert
                var fileId = Console.ReadLine();
                if (fileId == null)
                {
                    Console.WriteLine("Keine ID eingegeben");
                    return;
                }
                var file = await GetFileAsync("api/Files/" + fileId);
                if (file == null)
                {
                    Console.WriteLine("Die angegebene Datei-ID existiert nicht.");
                    return;
                }

                // Dateipfad muss eingegeben werden
                Console.WriteLine("Gib bitte den Pfad der Datei an, die du als neue Version hochladen möchtest.");
                var filePath = Console.ReadLine();

                // Lese neue Version der Datei ein
                VersionObject newVersion = new VersionObject
                {
                    // TODO: Text muss eingelesen werden
                    Filename = "testfile",
                    Text = "Das hier ist ein Text."
                };

                // Hole neueste Remote-Version der Datei
                var version = await GetVersionAsync("api/Versions/" + file.VersionIds.Max());

                // Vergleiche neueste Remote-Version der Datei mit dem lokalen Stand
                Console.WriteLine("Die folgenden Änderungen werden in einer neuen Version hochgeladen. Bitte bestätigen mit 'y'.");
                TextCompare textComparer = new TextCompare(version.Text, newVersion.Text);
                Console.WriteLine(textComparer.VergleicheObjekte());

                // Bestätigung erforderlich
                if(Console.ReadLine() != "y")
                {
                    Console.WriteLine("Die neue Version wurde nicht hochgeladen.");
                    return;
                }

                // Lade neue Version der Datei hoch und gib sie zurück
                var url = await CreateVersionAsync(newVersion);
                version = await GetVersionAsync(url.PathAndQuery);

                // Bearbeite Datei, sodass sie auch auf die soeben erstellte Version verweist
                var versionIds = file.VersionIds;
                versionIds.Add(version.Id);

                // Bearbeitete Datei hochladen
                file = await UpdateFileAsync(file);

                // Bestätigung
                Console.WriteLine("Für die Datei {0} (Datei-ID: {1}) wurde die neue Version {2} hochgeladen.", version.Filename, file.Id, version.Id);
            }
            else
            {
                // Hinweis
                Console.WriteLine("Es gibt noch keine Dateien, die du überschreiben könntest. Lege mit 'addfile' eine Neue an.");
            }
        }

        static async Task AddFile()
        {
            // Einfügen einer neuen Datei
            
            // Dateipfad muss eingegeben werden
            Console.WriteLine("Bitte gib den Pfad der Datei an, die du hochladen möchtest.");
            var filePath = Console.ReadLine();

            // Lese neue Version der Datei ein
            VersionObject newVersion = new VersionObject
            {
                // TODO: Text muss eingelesen werden
                Filename = "testfile",
                Text = "this is the text."
            };

            // Lade initiale Version der Datei hoch und gib sie zurück
            var url = await CreateVersionAsync(newVersion);
            var createdVersion = await GetVersionAsync(url.PathAndQuery);

            // Erstelle neue Datei, die auf die soeben erstellte Version verweist
            FileObject newFile = new FileObject
            {
                VersionIds = [createdVersion.Id]
            };

            // Lade neue Datei hoch und gib sie zurück
            url = await CreateFileAsync(newFile);
            var file = await GetFileAsync(url.PathAndQuery);

            // Bestätigung
            Console.WriteLine("Die Datei {0} (Datei-ID: {1}) wurde in der Version {2} hochgeladen.", createdVersion.Filename, file.Id, createdVersion.Id);
        }
        static async Task ResetFile()
        {
            // Hole alle Dateien vom Server und überprüfe, ob es überhautp welche gibt
            var remoteFiles = await GetFilesAsync();
            if (remoteFiles.Length != 0)
            {
                Console.WriteLine("Welche Datei möchtest du auf eine alte Version zurücksetzen? Gib bitte die Datei-ID an.");

                // Gib alle Dateien, aktuelle Dateinamen und Versions-IDs aus
                foreach (var remoteFile in remoteFiles)
                {
                    var remoteVersion = await GetVersionAsync("api/Versions/" + remoteFile.VersionIds.Max());

                    Console.WriteLine("  Datei-ID: {0}, Dateiname: {1}, aktuelle Version: {2}, Stand: {3}", remoteFile.Id, remoteVersion.Filename, remoteVersion.Id, remoteVersion.Timestamp);
                }

                // TODO: Überprüfen, ob Datei gesperrt ist und ob man eine neue Version anlegen darf!
                // Überprüfe, ob die eingegebene Datei-ID existiert
                var fileId = Console.ReadLine();
                if (fileId == null)
                {
                    Console.WriteLine("Keine ID eingegeben");
                    return;
                }
                var file = await GetFileAsync("api/Files/" + fileId);
                if (file == null)
                {
                    Console.WriteLine("Die angegebene Datei-ID existiert nicht.");
                    return;
                }

                // Alle Versionen ausgeben
                Console.WriteLine("Auf welche Verson möchtest du die Datei zurücksetzen? Gib bitte die Version an.");
                foreach (var versionId in file.VersionIds)
                {
                    var version = await GetVersionAsync("api/Versions/" + versionId);
                    Console.WriteLine("  Version: {0}, Dateiname: {1}, Stand: {2}", version.Id, version.Filename, version.Timestamp);
                }

                // Überprüfe, ob die eingegebene Versions-ID existiert
                var resetVersionId = Console.ReadLine();
                if (resetVersionId == null)
                {
                    Console.WriteLine("Keine Versions-ID eingegeben");
                    return;
                }
                var resetVersion = await GetVersionAsync("api/Versions/" + resetVersionId);
                if (resetVersion == null)
                {
                    Console.WriteLine("Die angegebene Versions-ID existiert nicht.");
                    return;
                }

                // Erstelle neue Version mit altem Dateinamen und Dateiinhalt
                VersionObject newVersion = new VersionObject
                {
                    Filename = resetVersion.Filename,
                    Text = resetVersion.Text
                };

                // Hole neueste Remote-Version der Datei
                var currentVersion = await GetVersionAsync("api/Versions/" + file.VersionIds.Max());

                // Vergleiche neueste Remote-Version der Datei mit dem lokalen Stand
                Console.WriteLine("Die folgenden Änderungen werden in einer neuen Version hochgeladen. Bitte bestätigen mit 'y'.");
                TextCompare textComparer = new TextCompare(currentVersion.Text, newVersion.Text);
                Console.WriteLine(textComparer.VergleicheObjekte());

                // Bestätigung erforderlich
                if (Console.ReadLine() != "y")
                {
                    Console.WriteLine("Die neue Version wurde nicht hochgeladen.");
                    return;
                }

                // Lade neue Version der Datei hoch und gib sie zurück
                var url = await CreateVersionAsync(newVersion);
                var createdVersion = await GetVersionAsync(url.PathAndQuery);

                // Bearbeite Datei, sodass sie auch auf die soeben erstellte Version verweist
                var versionIds = file.VersionIds;
                versionIds.Add(createdVersion.Id);

                // Bearbeitete Datei hochladen
                file = await UpdateFileAsync(file);

                // Bestätigung
                Console.WriteLine("Der alte Stand (Version {0}) der Datei {1} (Datei-ID: {2}) wurde in der neuen Version {3} hochgeladen.", resetVersion.Id, createdVersion.Filename, file.Id, createdVersion.Id);
            }
            else
            {
                // Hinweis
                Console.WriteLine("Es gibt noch keine Dateien, die du überschreiben könntest. Lege mit 'addfile' eine Neue an.");
            }
        }

        static async Task CreateTag()
        {
            // Kennzeichnen einer Version mit einem Tag

            // Hole alle Dateien vom Server und überprüfe, ob es überhautp welche gibt
            var remoteFiles = await GetFilesAsync();
            if (remoteFiles.Length != 0)
            {
                Console.WriteLine("Für welche Datei möchtest du einen Tag erstellen? Gib bitte die Datei-ID an.");

                // Gib alle Dateien, aktuelle Dateinamen und Versions-IDs aus
                foreach (var remoteFile in remoteFiles)
                {
                    var remoteVersion = await GetVersionAsync("api/Versions/" + remoteFile.VersionIds.Max());

                    Console.WriteLine("  Datei-ID: {0}, Dateiname: {1}, aktuelle Version: {2}, Stand: {3}", remoteFile.Id, remoteVersion.Filename, remoteVersion.Id, remoteVersion.Timestamp);
                }

                // Überprüfe, ob die eingegebene Datei-ID existiert
                var fileId = Console.ReadLine();
                if (fileId == null)
                {
                    Console.WriteLine("Keine ID eingegeben");
                    return;
                }
                var file = await GetFileAsync("api/Files/" + fileId);
                if (file == null)
                {
                    Console.WriteLine("Die angegebene Datei-ID existiert nicht.");
                    return;
                }

                // Tag eingeben
                Console.WriteLine("Wie soll der Tag heißen?");
                var tag = Console.ReadLine();

                // Bearbeite aktuelle Version, sodass der Tag überschrieben wird
                var version = await GetVersionAsync("api/Versions/" + file.VersionIds.Max());
                version.Tag = tag;

                // Bearbeitete Version hochladen
                version = await UpdateVersionAsync(version);

                // Bestätigung
                Console.WriteLine("Die Version {0} der Datei {1} (Datei-ID: {2}) hat jetzt den Tag {3}.", version.Id, version.Filename, file.Id, version.Tag);
            }
            else
            {
                // Hinweis
                Console.WriteLine("Es gibt noch keine Dateien, für die du einen Tag erstellen könntest. Lege mit 'addfile' eine Neue an.");
            }
        }
    }
}