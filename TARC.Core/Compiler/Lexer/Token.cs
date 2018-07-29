namespace TARC.Compiler
{
    public enum TokenType
    {
        LiteralString,
        LiteralRegex,

        OperationAssign,

        Directive,

        EndSequence
    }

    public class Token
    {
        public TokenType Type { get; set; }

        public string Value { get; set; } = string.Empty;

        public int LineNumber { get; set; }
        
        public int ColumnNumber { get; set; }

        public string Line { get; set; }

        public Token(TokenType type)
        {
            Type = type;
        }

        public Token(TokenType type, Line line)
        {
            Type = type;

            Line = line.Text;
            LineNumber = line.LineNumber;
        }

        public override string ToString()
        {
            //string value = "";

            //if (Value != "")
            //    value = " | " + Value;

            //string message = Enum.GetName(typeof(TokenType), Type) + value;
            //{message}\r\n

            string totalMessage = $"at line {LineNumber}, column {ColumnNumber}\r\n{Line}\r\n";

            totalMessage += "^".PadLeft(ColumnNumber + 1, ' ');

            return totalMessage;
        }
    }
}
