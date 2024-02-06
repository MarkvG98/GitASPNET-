using System.Diagnostics;

namespace GitProjektWHS
{
    public class TextCompare
    {
        /*
       möglicher Aufruf :
        TextCompare textComparer = new TextCompare(
      "Hallo\r\n" +
      "Ich bin Mark\r\n" +
      "Ich wohne in Bocholt\r\n" +
      "Ich hasse Buchhaltung\r\n"
      ,

      "Hallo\r\n" +
      "Ich bin Vesko\r\n" +
      "Ich wohne in Bocholt\r\n" +
      "Ich Staubsaugegerne\r\n" +
      "Ich bin Toll\r\n" +
      "Ich hasse Buchhaltung\r\n"
      );
        textComparer.VergleicheObjekte();
        */
        private string[] OldText;
        private string[] newText;

        public TextCompare(string text1, string text2)
        {
            this.OldText = text1.Split('\n');
            this.newText = text2.Split('\n');
        }

        public (string added, string removed) VergleicheObjekte()
        {
            var RemovedLines = string.Join(string.Empty, OldText.Except(newText).ToArray());
            var AddedLines = string.Join(string.Empty, newText.Except(OldText).ToArray());

            Debug.WriteLine("Removed");
            Debug.WriteLine(RemovedLines);
            Debug.WriteLine("Added");
            Debug.WriteLine(AddedLines);

            return (AddedLines, RemovedLines);
        }
    }
}