using Commons.Models;
using System;
using System.Net;
using System.Net.Http.Headers;

namespace Client
{
    class Program
    {
        static HttpClient client = new HttpClient();

        static void ShowVersion(VersionObject version)
        {
            Console.WriteLine($"ID: {version.Id}\tTag: " + $"{version.Tag}");
        }

        static async Task<Uri> CreateVersionAsync(VersionObject version)
        {
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

        static async Task<VersionObject[]> GetVersionsAsync()
        {
            VersionObject[] versions = null;
            HttpResponseMessage response = await client.GetAsync("api/Versions");
            if (response.IsSuccessStatusCode)
            {
                versions = await response.Content.ReadAsAsync<VersionObject[]>();
            }
            return versions;
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
                $"api/Versions/{file.Id}", file);
            response.EnsureSuccessStatusCode();

            // Deserialize the updated product from the response body.
            file = await response.Content.ReadAsAsync<FileObject>();
            return file;
        }

        static async Task<HttpStatusCode> DeleteVersionAsync(long id)
        {
            HttpResponseMessage response = await client.DeleteAsync(
                $"api/Versions/{id}");
            return response.StatusCode;
        }
        static void ResetVersion()
        {
            Console.WriteLine("Gib die Versionsdatei an");
            var VersionsID = Console.ReadLine();
            Console.WriteLine("Gib die Version an");
            var Version = Console.ReadLine();

            Console.WriteLine("Du Möchtest Datei {0} auf Version {1} " +
                "zurücksetzen? Bitte mit J bestätigen", VersionsID, Version);

            if (Console.ReadLine() == "J")
            {
                //Mache Reset
                Console.WriteLine("Reset erfolgreich");

            }
            else
            {
                Console.WriteLine("Abbruch");
            }

        }

        static void Main()
        {
            // RunAsync().GetAwaiter().GetResult();

            client.BaseAddress = new Uri("http://localhost:5049/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            Console.WriteLine("Hallo zum GitProjektWHS! Für eine Liste aller Befehle bitte 'help' eingeben.");

            HandleUserInput();

        }

        private static void HandleUserInput()
        {
            var userInput = Console.ReadLine();

            switch (userInput)
            {
                case "savefile":
                    SaveFile();
                    break;
                case "getfile":
                    // Holen der neuesten Version einer Datei vom Server
                    break;
                case "getfilewithlock":
                    // Holen der neuesten Version einer Datei mit Sperren vom Server
                    break;
                case "addfile":
                    AddFile();
                    break;
                case "resetfile":
                    // Zurücksetzen  einer Datei auf eine alte Version
                    break;
                case "createtag":
                    // Kennzeichnen einer Version mit einem Tag
                    break;
                case "help":
                    // Befehle auflisten
                    Console.WriteLine("Befehle:\n" +
                                      "  savefile         Speichern einer Datei in einer neuen Version\n" +
                                      "  getfile          Holen der neuesten Version einer Datei vom Server\n" +
                                      "  getfilewithlock  Holen der neuesten Version einer Datei mit Sperren vom Server\n" +
                                      "  addfile          Einfügen einer neuen Datei\n" +
                                      "  resetfile        Zurücksetzen  einer Datei auf eine alte Version\n" +
                                      "  createtag        Kennzeichnen einer Version mit einem Tag\n" +
                                      "  help             Befehle auflisten");
                    break;
                default:
                    Console.WriteLine("Unbekannter Befehl. Für eine Liste aller Befehle bitte 'help' eingeben.");
                    break;
            }

            HandleUserInput();
        }

        static async Task SaveFile()
        {
            // Speichern einer Datei in einer neuen Version

            var remoteFiles = await GetFilesAsync();

            if (remoteFiles.Length != 0)
            {
                Console.WriteLine("Für welche Datei möchtest du eine neue Version hochladen? Gib bitte die Datei-ID an.");

                foreach (var remoteFile in remoteFiles)
                {
                    var remoteVersion = await GetVersionAsync("api/Versions/" + remoteFile.VersionIds.Max());

                    Console.WriteLine("  Datei-ID: {0}, Dateiname: {1}, aktuelle Version: {2})", remoteFile.Id, remoteVersion.Filename, remoteVersion.Id);
                }

                Console.WriteLine("Gib bitte den Pfad der Datei an, die du als neue Version hochladen möchtest.");

                var fileId = Console.ReadLine();
                var file = await GetFileAsync("api/Files" + fileId);

                VersionObject newVersion = new VersionObject
                {
                    // TODO: Text muss eingelesen werden
                    Filename = "testfile",
                    Text = "this is the text."
                };

                var url = await CreateVersionAsync(newVersion);
                var version = await GetVersionAsync(url.PathAndQuery);

                var versionIds = file.VersionIds;
                versionIds[versionIds.Length] = version.Id;

                FileObject updatedFile = new FileObject
                {
                    VersionIds = versionIds
                };

                file = await UpdateFileAsync(file);

                Console.WriteLine("Für die Datei {0} (ID: {1}) wurde in der neuen Version {2} hochgeladen.", version.Filename, file.Id, version.Id);
            }
            else
            {
                Console.WriteLine("Es gibt noch keine Dateien, die du überschreiben könntest. Lege mit 'addfile' eine neue an.");
            }
        }

        static async Task AddFile()
        {
            // Einfügen einer neuen Datei

            Console.WriteLine("Bitte gib den Pfad der Datei an, die du hochladen möchtest.");

            var filePath = Console.ReadLine();

            VersionObject newVersion = new VersionObject
            {
                // TODO: Text muss eingelesen werden
                Filename = "testfile",
                Text = "this is the text."
            };

            var url = await CreateVersionAsync(newVersion);
            var version = await GetVersionAsync(url.PathAndQuery);

            FileObject newFile = new FileObject
            {
                VersionIds = [version.Id]
            };

            url = await CreateFileAsync(newFile);
            var file = await GetFileAsync(url.PathAndQuery);

            Console.WriteLine("Die Datei {0} (ID: {1}) wurde in der Version {2} hochgeladen.", version.Filename, file.Id, version.Id);
        }
    }
}