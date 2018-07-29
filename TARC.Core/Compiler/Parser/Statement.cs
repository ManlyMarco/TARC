using System.Collections.Generic;
using System.Linq;

namespace TARC.Compiler
{
    public enum StatementType
    {
        TranslationEntry,

        DirectiveSet,
    }

    public class Statement
    {
        public StatementType Type { get; protected set; }

        public IList<string> Arguments { get; protected set; }

        public Statement(StatementType type, params string[] arguments)
        {
            Type = type;
            Arguments = arguments.ToList();
        }
    }
}
