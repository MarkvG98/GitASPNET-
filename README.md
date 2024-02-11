# GitASPNET-

Eine einfache Anwendung zur Versionsverwaltung von Textdateien. Abschlussprojekt der Lehrveranstaltung Software Engineering I im Wintersemester 2023/24.

## Installation

Projekt klonen

```bash
git clone https://github.com/MarkvG98/GitASPNET-.git
```
Microsoft SQL Server installieren

[https://www.microsoft.com/de-de/sql-server/sql-server-downloads](https://www.microsoft.com/de-de/sql-server/sql-server-downloads)

## Usage

### Versionsverwaltung

```bash
# Speichern einer Datei in einer neuen Version
savefile

# Holen der neuesten Version einer Datei vom Server
getfile

# Holen der neuesten Version einer Datei mit Sperren vom Server
getfilewithlock

# Einfügen einer neuen Datei
addfile

# Einfügen aller Dateien aus einem Ordner
addrepo

# Zurücksetzen einer Datei auf eine alte Version
resetfile

# Kennzeichnen einer Version mit einem Tag
edittag

# Befehle auflisten
help
```

### Datenbank

In der Paket-Manager-Konsole:

```bash
# Datenbankstrukturänderungen erfassen
Add-Migration

# Datenbankstrukturänderungen anwenden 
Update-Database
```