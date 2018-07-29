namespace TARC
{
    internal static class Utility
    {
        public static string CurrentLine(string text, int lineNumber)
        {
            int index = 0;
            int currentLineNumber = 0;
            
            foreach (char character in text)
            {
                if (lineNumber <= currentLineNumber)
                    break;

                if (character == '\n')
                    currentLineNumber++;

                index++;
            }

            int globalIndex = text.IndexOf('\n', index);

            if (globalIndex < 0)
                globalIndex = text.Length;

            return text.Substring(index, globalIndex - index);
        }
    }
}
