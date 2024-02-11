using System.Diagnostics;

namespace Client
{
    public class TextCompare(string oldText, string newText)
    {

        private readonly string[] OldText = oldText.Split('\n');
        private readonly string[] newText = newText.Split('\n');

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