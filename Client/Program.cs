using Commons.Models;
using System.Net;
using System.Net.Http.Headers;

namespace Client
{
    class Program
    {
        static HttpClient client = new HttpClient();

        static void ShowVersion(VersionObject version)
        {
            Console.WriteLine($"ID: {version.Id}\tDateien: " +
                $"{version.FileId}\tTag: " + $"{version.Tag}");
        }

        static async Task<Uri> CreateVersionAsync(VersionObject version)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync(
                "api/Versions", version);
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

            Console.WriteLine(@"Hallo zum Git ""help"" für hilfe eingeben");

            while (true)
            {
                GetUserCommand();
            }

        }

        private static void GetUserCommand()
        {
            var userIput = Console.ReadLine();
            VersionObject version = new VersionObject
            {
                Id = 1
            };

            switch (userIput)
            {
                case "DeleteVersionAsync":
                    CreateVersionAsync(version);
                    break;
                case "UpdateVersionAsync":
                    UpdateVersionAsync(version);
                    break;
                case "Reset":
                    ResetVersion();
                    break;
                case "help":
                    Console.WriteLine("hier alle Methoden nennen");
                    break;
                default:
                    Console.WriteLine("Ungültige Eingabe");
                    break;
            }
        }

        static async Task RunAsync()
        {
            client.BaseAddress = new Uri("https://localhost:7158/");
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