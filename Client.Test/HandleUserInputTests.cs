using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Test
{
    public class HandleUserInputTests
    {
        [Fact]
      
            public async Task HandleUserInput_UnknownCommand_PrintsErrorMessage()
            {
                // Arrange
                var userInput = "unknowncommand";
                var expectedOutput = "Unbekannter Befehl. Für eine Liste aller Befehle bitte 'help' eingeben.";
                var consoleOutput = new StringWriter();
                Console.SetOut(consoleOutput);

                // Act
                await Program.HandleUserInput(userInput);

                // Assert
                expectedOutput.Should().Be(consoleOutput.ToString().Trim());
            }
        [Fact]

        public async Task HandleUserInput_HelpCommand_PrintsHelpMessage()
        {
            // Arrange
            var userInput = "help";
            var expectedOutput = "Befehle:\n" +
                                      "  savefile         Speichern einer Datei in einer neuen Version\n" +
                                      "  getfile          Holen der neuesten Version einer Datei vom Server\n" +
                                      "  getfilewithlock  Holen der neuesten Version einer Datei mit Sperren vom Server\n" +
                                      "  addfile          Einfügen einer neuen Datei\n" +
                                      "  resetfile        Zurücksetzen einer Datei auf eine alte Version\n" +
                                      "  edittag          Kennzeichnen einer Version mit einem Tag\n" +
                                      "  help             Befehle auflisten";
            var consoleOutput = new StringWriter();
            Console.SetOut(consoleOutput);

            // Act
            await Program.HandleUserInput(userInput);

            // Assert
            expectedOutput.Should().Be(consoleOutput.ToString().Trim());
        }


    }
}
