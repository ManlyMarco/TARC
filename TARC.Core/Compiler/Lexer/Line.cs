namespace TARC.Compiler
{
    public struct Line
    {
        public string Text;
        public int LineNumber;

        public Line(string text, int lineNumber)
        {
            Text = text;
            LineNumber = lineNumber;
        }
    }
}
