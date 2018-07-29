using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TARC.Compiler
{
    public class MarkupCompiler
    {
        protected class CompilerThreadContext
        {
            public Lexer Lexer = new Lexer();
            public Parser Parser = new Parser();
        }

        public Archive CompileArchive(string directory)
        {
            Archive archive = new Archive
            {
                Compression = ArchiveCompression.Uncompressed,
                OriginalEncoding = ArchiveEncoding.UTF8,
                TranslatedEncoding = ArchiveEncoding.UTF16
            };

            foreach (string file in Directory.GetFiles(directory, "*.txt", SearchOption.AllDirectories))
            {
                var sections = CompileFile(file, new CompilerThreadContext());

                foreach (Section section in sections)
                {
                    var currentSection = archive.Sections.FirstOrDefault(x => x.Exe == section.Exe);

                    if (currentSection == null)
                    {
                        archive.Sections.Add(section);
                    }
                    else
                    {
                        currentSection.Lines.AddRange(section.Lines);
                    }
                }
            }

            return archive;
        }

        protected IEnumerable<Section> CompileFile(string filePath, CompilerThreadContext context)
        {
            string totalText = File.ReadAllText(filePath);

            IEnumerable<Token> tokens = context.Lexer.Process(totalText);
            
            IEnumerable<Statement> statements = context.Parser.Parse(tokens);

            return Compile(statements, out IEnumerable<string> messages);
        }

        protected IEnumerable<Section> Compile(IEnumerable<Statement> statements, out IEnumerable<string> messages)
        {
            List<Section> sections = new List<Section>();
            List<string> messagesList = new List<string>();

            byte[] levels = { 255 };
            bool convertWideNumbers = false;
            bool? useAutosize = null;
            bool? allowOverflow = null;
            byte? fontsize = null;
            string exe = "all";

            bool AssertDirectiveArgument(Statement statement)
            {
                if (statement.Arguments.Count != 2)
                {
                    messagesList.Add($"WARNING: Expected 2 arguments for '#set {statement.Arguments[0]}', ignoring");
                    return false;
                }

                return true;
            }

            foreach (Statement statement in statements)
            {
                switch (statement.Type)
                {
                    case StatementType.DirectiveSet:
                        switch (statement.Arguments[0].ToLower())
                        {
                            case "exe":
                                if (AssertDirectiveArgument(statement))
                                    exe = statement.Arguments[1];
                                break;
                            case "level":
                                if (AssertDirectiveArgument(statement))
                                    levels = statement.Arguments[1].Split(',').Select(x => x.StartsWith("-") ? "255" : x).Select(byte.Parse).ToArray();
                                break;
                            case "autosize":
                                if (AssertDirectiveArgument(statement))
                                    useAutosize = statement.Arguments[1].ToLower() == "default" ? (bool?)null : bool.Parse(statement.Arguments[1].ToLower());
                                break;
                            case "overflow":
                                if (AssertDirectiveArgument(statement))
                                    allowOverflow = statement.Arguments[1].ToLower() == "default" ? (bool?)null : bool.Parse(statement.Arguments[1].ToLower());
                                break;
                            case "fontsize":
                                if (AssertDirectiveArgument(statement))
                                    fontsize = statement.Arguments[1].ToLower() == "default" ? (byte?)null : byte.Parse(statement.Arguments[1].ToLower());
                                break;
                            case "widenumbers":
                                if (AssertDirectiveArgument(statement))
                                    convertWideNumbers = bool.Parse(statement.Arguments[1].ToLower());
                                break;
                        }
                        break;
                    case StatementType.TranslationEntry:
                        CompiledLine line = new CompiledLine
                        {
                            OriginalLine = statement.Arguments[1],
                            TranslatedLine = statement.Arguments[3],
                        };
                        
                        line.Flags.IsOriginalRegex = bool.Parse(statement.Arguments[0]);
                        line.Flags.IsTranslationRegex = bool.Parse(statement.Arguments[2]);

                        line.Flags.AllowOverflow = allowOverflow;
                        line.Flags.UseAutosize = useAutosize;
                        line.Flags.ConvertWideNumbers = convertWideNumbers;

                        line.FontSize = fontsize;
                        line.Levels = (byte[])levels.Clone();

                        Section section = sections.FirstOrDefault(x => x.Exe.Equals(exe, StringComparison.OrdinalIgnoreCase));

                        if (section == null)
                        {
                            section = new Section
                            {
                                Exe = exe
                            };

                            sections.Add(section);
                        }

                        section.Lines.Add(line);

                        break;
                }
            }

            messages = messagesList;
            return sections;
        }
    }
}
