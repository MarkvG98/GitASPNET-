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

        static async Task<VersionObject> UpdateVersionAsync(VersionObject version)
        {
            HttpResponseMessage response = await client.PutAsJsonAsync(
                $"api/Versions/{version.Id}", version);
            response.EnsureSuccessStatusCode();

            // Deserialize the updated product from the response body.
            version = await response.Content.ReadAsAsync<VersionObject>();
            return version;
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

            Console.WriteLine("Hallo zum GitProjektWHS! Für eine Liste aller Befehle bitte 'help' eingeben.");

            HandleUserInput();

        }

        private static void HandleUserInput()
        {
            var userInput = Console.ReadLine();
            VersionObject version = new VersionObject
            {
                Id = 1
            };

            switch (userInput)
            {
                case "savefile":
                    // Speichern einer Datei in einer neuen Version
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

        static async Task AddFile()
        {
            // Einfügen einer neuen Datei

            Console.WriteLine("Bitte gib den Pfad der Datei an, die du hochladen möchtest.");

            var filePath = Console.ReadLine();

            client.BaseAddress = new Uri("http://localhost:5049/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            VersionObject version = new VersionObject
            {
                // TODO: Text muss eingelesen werden
                Filename = "testfile",
                Text = "this is the text."
            };

            var url = await CreateVersionAsync(version);
            version = await GetVersionAsync(url.PathAndQuery);

            FileObject file = new FileObject
            {
                Locked = false,
                VersionIds = [version.Id]
            };

            url = await CreateFileAsync(file);
            file = await GetFileAsync(url.PathAndQuery);

            Console.WriteLine("Die Datei {0} wurde in der Version {1} hochgeladen.", version.Filename, version.Id);
        }

        static async Task RunAsync()
        {
            client.BaseAddress = new Uri("http://localhost:5049/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                // Create a new version
                VersionObject version = new VersionObject
                {
                    Id = 1
                };

                var url = await CreateVersionAsync(version);
                Console.WriteLine($"Created at {url}");

                // Get the version
                version = await GetVersionAsync(url.PathAndQuery);
                ShowVersion(version);

                // Update the version
                Console.WriteLine("Updating tag...");
                version.Tag = "v1.0.0";
                await UpdateVersionAsync(version);

                // Get the updated version
                version = await GetVersionAsync(url.PathAndQuery);
                ShowVersion(version);

                // Delete the version
                var statusCode = await DeleteVersionAsync(version.Id);
                Console.WriteLine($"Deleted (HTTP Status = {(int)statusCode})");

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ReadLine();
        }
    }
}