using System;
using System.Collections.Generic;
using System.Linq;

namespace TARC.Compiler
{
    public class Parser
    {
        public IEnumerable<Statement> Parse(IEnumerable<Token> tokens)
        {
            List<Token> currentTokenList = new List<Token>();
            List<List<Token>> splitTokens = new List<List<Token>>();

            foreach (Token token in tokens)
            {
                currentTokenList.Add(token);

                if (token.Type == TokenType.EndSequence)
                {
                    splitTokens.Add(currentTokenList);
                    currentTokenList = new List<Token>();
                }
            }

            splitTokens.Add(currentTokenList);

            //return splitTokens.AsParallel().SelectMany(ParseSequence).AsSequential();
            //return splitTokens.SelectMany(ParseSequence);

            List<Statement> statements = new List<Statement>();
            foreach (var tokenList in splitTokens)
            {
                statements.AddRange(ParseSequence(tokenList));
            }

            return statements;
        }

        public IEnumerable<Statement> ParseSequence(IEnumerable<Token> tokens) => ParseSequence(tokens.ToList());
        public IEnumerable<Statement> ParseSequence(IList<Token> tokens)
        {
            List<Statement> statements = new List<Statement>();

            if (tokens.Count == 0 || tokens[0].Type == TokenType.EndSequence)
            {
                return statements;
            }
            else if (tokens[tokens.Count - 1].Type != TokenType.EndSequence)
            {
                throw new ParserException("Was expecting end of sequence", tokens[tokens.Count - 1]);
            }

            if (tokens[0].Type == TokenType.Directive)
            {
                StatementType type;
                List<string> arguments = new List<string>();

                if (tokens[0].Value.Equals("set", StringComparison.OrdinalIgnoreCase))
                {
                    if (tokens.Count < 4)
                        throw new ParserException("Was expecting string literal", tokens[tokens.Count - 1]);
                    else if (tokens.Count > 4)
                        throw new ParserException("Was expecting end of sequence", tokens[3]);

                    for (int i = 1; i < 3; i++)
                    {
                        if (tokens[i].Type != TokenType.LiteralString)
                            throw new ParserException("Was expecting string literal", tokens[i]);
                    }

                    type = StatementType.DirectiveSet;

                    arguments.Add(tokens[1].Value);
                    arguments.Add(tokens[2].Value);
                }
                else
                    throw new ParserException($"Unknown directive: {tokens[0].Value}", tokens[0]);

                statements.Add(new Statement(type, arguments.ToArray()));
            }
            else if (tokens[0].Type == TokenType.LiteralString || tokens[0].Type == TokenType.LiteralRegex)
            {
                bool isTranslatedSection = false;
                bool expectingEnd = false;

                bool isOriginalRegex = false;
                bool isTranslatedRegex = false;

                string originalString = null;
                string translatedString = null;

                foreach (Token token in tokens)
                {
                    if (expectingEnd && !isTranslatedSection)
                    {
                        if (token.Type != TokenType.OperationAssign)
                            throw new ParserException("Was expecting an assignment", token);
                    }
                    else if (expectingEnd && isTranslatedSection)
                    {
                        if (token.Type != TokenType.EndSequence)
                            throw new ParserException("Was expecting end of sequence", token);
                    }
                    else
                    {
                        if (!isTranslatedSection && !isOriginalRegex && token.Type == TokenType.LiteralRegex && originalString != null)
                            throw new ParserException("Was not expecting a regex", token);
                        else if (isTranslatedSection && !isTranslatedRegex && token.Type == TokenType.LiteralRegex && translatedString != null)
                            throw new ParserException("Was not expecting a regex", token);
                    }

                    if (token.Type == TokenType.EndSequence)
                    {
                        if (!isTranslatedSection || originalString == null || translatedString == null)
                        {
                            throw new ParserException("Was not expecting end of sequence", token);
                        }
                        
                        expectingEnd = false;
                        break;
                    }

                    if (token.Type == TokenType.OperationAssign)
                    {
                        if (isTranslatedSection)
                        {
                            throw new ParserException("Was expecting end of sequence", token);
                        }

                        isTranslatedSection = true;
                        expectingEnd = false;
                        continue;
                    }

                    if (token.Type == TokenType.LiteralString)
                    {
                        if (!isTranslatedSection)
                        {
                            if (originalString == null)
                                originalString = token.Value;
                            else
                                originalString = $"{(originalString ?? "")} {token.Value}";
                        }
                        else
                        {
                            if (translatedString == null)
                                translatedString = token.Value;
                            else
                                translatedString = $"{(translatedString ?? "")} {token.Value}";
                        }
                    }
                    else if (token.Type == TokenType.LiteralRegex)
                    {
                        if (!isTranslatedSection)
                        {
                            originalString = token.Value;
                            isOriginalRegex = true;
                        }
                        else
                        {
                            translatedString = token.Value;
                            isTranslatedRegex = true;
                        }

                        expectingEnd = true;
                    }
                }

                statements.Add(new Statement(StatementType.TranslationEntry, isOriginalRegex.ToString(), originalString, isTranslatedRegex.ToString(), translatedString));
            }

            return statements;
        }
    }

    public class ParserException : Exception
    {
        public ParserException(string message, Token token) : base($"{message}\r\n{token.ToString()}")
        {

        }
    }
}
