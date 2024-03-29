﻿using Commons.Models;
using System;
using System.Net;
using System.Net.Http.Headers;

namespace Client
{
    public class Program
    {
        static readonly HttpClient client = new();

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
            while (true)
            {
                var userInput = Console.ReadLine();
                HandleUserInput(userInput).Wait();
            }
        }

        public static async Task HandleUserInput(string piUserInput)
        {
            // Warte auf Befehlseingabe

            // Verarbeite Befehl
            switch (piUserInput)
            {
                case "savefile":
                    await SaveFileAsync();
                    break;
                case "getfile":
                    await GetFileAsyns();
                    break;
                case "getfilewithlock":
                    await GetFileWithLockAsync();
                    break;
                case "getrepo":
                    await GetRepoAsync();
                    break;
                case "addfile":
                    await AddFileAsync();
                    break;
                case "addrepo":
                    await AddRepoAsync();
                    break;
                case "resetfile":
                    await ResetFileAsync();
                    break;
                case "edittag":
                    await EditTagAsync();
                    break;
                case "help":
                    // Befehle auflisten
                    Console.WriteLine("Befehle:\n" +
                                      "  savefile         Speichern einer Datei in einer neuen Version\n" +
                                      "  getfile          Holen der neuesten Version einer Datei vom Server\n" +
                                      "  getfilewithlock  Holen der neuesten Version einer Datei mit Sperren vom Server\n" +
                                      "  getrepo          Holen der neuesten Version aller Dateien vom Server\n" +
                                      "  addfile          Einfügen einer neuen Datei\n" +
                                      "  addrepo          Einfügen aller Dateien aus einem Ordner\n" +
                                      "  resetfile        Zurücksetzen einer Datei auf eine alte Version\n" +
                                      "  edittag          Kennzeichnen einer Version mit einem Tag\n" +
                                      "  help             Befehle auflisten");
                    break;
                default:
                    Console.WriteLine("Unbekannter Befehl. Für eine Liste aller Befehle bitte 'help' eingeben.");
                    break;
            }

        }

        private static async Task GetFileAsyns()
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
                if (fileId == "")
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

        private static async Task GetFileWithLockAsync()
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
                if (fileId == "")
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

        private static async Task GetRepoAsync()
        {
            // Holen der neuesten Version aller Dateien vom Server

            // Hole alle Dateien vom Server und überprüfe, ob es überhautp welche gibt
            var remoteFiles = await GetFilesAsync();
            if (remoteFiles.Length != 0)
            {
                // Speicherort eingeben
                Console.WriteLine("In welchem Ordner sollen die Dateien gespeichert werden?");
                var filePath = Console.ReadLine();

                // Gib alle Dateien, aktuelle Dateinamen und Versions-IDs aus
                foreach (var remoteFile in remoteFiles)
                {
                    var remoteVersion = await GetVersionAsync("api/Versions/" + remoteFile.VersionIds.Max());

                    // Erstelle die Datei samt Inhalt
                    try
                    {
                        File.WriteAllText(filePath + "\\" + remoteVersion.Filename, remoteVersion.Text);
                    }
                    catch
                    {
                        Console.WriteLine("Ungültiger Ordnerpfad");
                        return;
                    }

                    // Bestätigung
                    Console.WriteLine("  Die Datei {0} (Datei-ID: {1}) wurde in Version {2} heruntergeladen.", remoteVersion.Filename, remoteFile.Id, remoteVersion.Id);
                }
                // Bestätigung
                Console.WriteLine("Alle Dateien erfolgreich heruntergeladen");
            }
            else
            {
                // Hinweis
                Console.WriteLine("Es gibt noch keine Dateien, die du herunterladen könntest. Lege mit 'addfile' eine Neue an.");
            }
        }

        private static async Task SaveFileAsync()
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

                // Überprüfe, ob die eingegebene Datei-ID existiert
                var fileId = Console.ReadLine();
                if (fileId == "")
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

                // Überprüfe, ob Datei gesperrt ist
                if (file.Locked)
                {
                    Console.WriteLine("Die angegebene Datei ist gesperrt. Möchtest du sie entsperren, um sie zu bearbeiten? Bitte bestätigen mit 'y'.");
                    if (Console.ReadLine() != "y")
                    {
                        Console.WriteLine("Die neue Version wurde nicht hochgeladen.");
                        return;
                    }
                    file.Locked = false;
                }

                // Dateipfad muss eingegeben werden
                Console.WriteLine("Gib bitte den Pfad der Datei an, die du als neue Version hochladen möchtest.");
                var filePath = Console.ReadLine();

                // Lese neue Version der Datei ein
                string filename;
                string text;
                try
                {
                    filename = Path.GetFileName(filePath);
                    using var sr = new StreamReader(filePath);
                    text = sr.ReadToEnd();
                }
                catch
                {
                    Console.WriteLine("Ungültiger Dateipfad");
                    return;
                }

                // Speichere eingelesene Daten
                VersionObject newVersion = new()
                {
                    Filename = filename,
                    Text = text
                };

                // Hole neueste Remote-Version der Datei
                var version = await GetVersionAsync("api/Versions/" + file.VersionIds.Max());

                // Vergleiche neueste Remote-Version der Datei mit dem lokalen Stand
                Console.WriteLine("Die folgenden Änderungen werden in einer neuen Version hochgeladen. Bitte bestätigen mit 'y'.");
                TextCompare textComparer = new TextCompare(version.Text, newVersion.Text);
                Console.WriteLine("  Hinzugefügt:");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("    " + textComparer.VergleicheObjekte().added);
                Console.ResetColor();
                Console.WriteLine("  Entfernt:");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("    " + textComparer.VergleicheObjekte().removed);
                Console.ResetColor();

                // Bestätigung erforderlich
                if (Console.ReadLine() != "y")
                {
                    Console.WriteLine("Die neue Version wurde nicht hochgeladen.");
                    return;
                }

                // Lade neue Version der Datei hoch und gib sie zurück
                var url = await CreateVersionAsync(newVersion);
                version = await GetVersionAsync(url.PathAndQuery);

                // Bearbeite Datei, sodass sie auch auf die soeben erstellte Version verweist
                file.VersionIds.Add(version.Id);

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

        private static async Task AddFileAsync()
        {
            // Einfügen einer neuen Datei

            // Dateipfad muss eingegeben werden
            Console.WriteLine("Bitte gib den Pfad der Datei an, die du hochladen möchtest.");
            var filePath = Console.ReadLine();

            // Lese Datei ein
            string filename;
            string text;
            try
            {
                filename = Path.GetFileName(filePath);
                using var sr = new StreamReader(filePath);
                text = sr.ReadToEnd();
            }
            catch
            {
                Console.WriteLine("Ungültiger Dateipfad");
                return;
            }

            // Speichere eingelesene Daten
            VersionObject newVersion = new()
            {
                Filename = filename,
                Text = text
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

        private static async Task AddRepoAsync()
        {
            // Einfügen aller Dateien aus einem Ordner

            // Ordnerpfad muss eingegeben werden
            Console.WriteLine("Bitte gib den Pfad des Ordners an, aus dem du alle Dateien hochladen möchtest.");
            var repoPath = Console.ReadLine();

            // Hole alle Dateien
            DirectoryInfo dirInfo = new(repoPath);
            var files = dirInfo.GetFiles();

            try
            {
                foreach (var file in files)
                {
                    // Lese Datei ein
                    string filename;
                    string text;
                    try
                    {
                        filename = Path.GetFileName(file.FullName);
                        using var sr = new StreamReader(file.FullName);
                        text = sr.ReadToEnd();
                    }
                    catch
                    {
                        Console.WriteLine("Ungültiger Dateipfad");
                        return;
                    }

                    // Speichere eingelesene Daten
                    VersionObject newVersion = new()
                    {
                        Filename = filename,
                        Text = text
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
                    var createdFile = await GetFileAsync(url.PathAndQuery);

                    // Bestätigung
                    Console.WriteLine("Die Datei {0} (Datei-ID: {1}) wurde in der Version {2} hochgeladen.", createdVersion.Filename, createdFile.Id, createdVersion.Id);
                }
            }
            catch
            {
                Console.WriteLine("Ungültiger Ordnerpfad");
            }
        }

        private static async Task ResetFileAsync()
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

                // Überprüfe, ob die eingegebene Datei-ID existiert
                var fileId = Console.ReadLine();
                if (fileId == "")
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

                // Überprüfe, ob Datei gesperrt ist
                if (file.Locked)
                {
                    Console.WriteLine("Die angegebene Datei ist gesperrt. Möchtest du sie entsperren, um sie zu bearbeiten? Bitte bestätigen mit 'y'.");
                    if (Console.ReadLine() != "y")
                    {
                        Console.WriteLine("Die neue Version wurde nicht zurückgesetzt.");
                        return;
                    }
                    file.Locked = false;
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
                if (resetVersionId == "")
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
                VersionObject newVersion = new()
                {
                    Filename = resetVersion.Filename,
                    Text = resetVersion.Text
                };

                // Hole neueste Remote-Version der Datei
                var currentVersion = await GetVersionAsync("api/Versions/" + file.VersionIds.Max());

                // Vergleiche neueste Remote-Version der Datei mit dem lokalen Stand
                Console.WriteLine("Die folgenden Änderungen werden in einer neuen Version hochgeladen. Bitte bestätigen mit 'y'.");
                TextCompare textComparer = new(currentVersion.Text, newVersion.Text);
                Console.WriteLine("  Hinzugefügt:");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("    " + textComparer.VergleicheObjekte().added);
                Console.ResetColor();
                Console.WriteLine("  Entfernt:");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("    " + textComparer.VergleicheObjekte().removed);
                Console.ResetColor();

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
                file.VersionIds.Add(createdVersion.Id);

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

        private static async Task EditTagAsync()
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
                if (fileId == "")
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