using System.Diagnostics;

namespace Client
{
    public class TextCompare(string text1, string text2)
    {
        /*
        möglicher Aufruf:
        TextCompare textComparer = new(text1, text2);
        textComparer.VergleicheObjekte();
        */

        private readonly string[] OldText = text1.Split('\n');
        private readonly string[] newText = text2.Split('\n');

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