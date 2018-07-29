using System;
using System.Collections.Generic;
using System.Text;

namespace TARC.Compiler
{
    public class Lexer
    {
        public IEnumerable<Token> Process(string totalString)
        {
            return TokenizeMarkup(Preprocess(totalString));
        }

        /// <summary>
        /// Split a string into sequences
        /// </summary>
        /// <param name="totalString"></param>
        /// <returns></returns>
        public IEnumerable<Line> Preprocess(string totalString)
        {
            int totalLength = totalString.Length;
            StringBuilder /* electric */ bodyBuilder /* from russia */ = new StringBuilder();
            int lineNumber = 0;

            void skipToEndOfLine(ref int i)
            {
                while (i < totalLength && totalString[i] != '\n')
                {
                    i++;
                }
            }

            bool isWideComment = false;

            for (int i = 0; i < totalLength; i++)
            {
                if (totalString[i] == '\r')
                    continue; //skip on carriage return

                if (i != totalLength - 1) //can't parse special comment stuff when on the last character
                {
                    char first = totalString[i];
                    char second = totalString[i + 1];
                    
                    if (first == '*' && second == '/') //end wide comment
                    {
                        isWideComment = false;
                        i++; //skip second character
                        continue;
                    }
                    else if (first == '/' && second == '*') //begin wide comment
                    {
                        isWideComment = true;
                        i++; //skip second character
                        continue;
                    }
                    else if (first == '/' && second == '/') //line comment
                    {
                        skipToEndOfLine(ref i);

	                    if (i >= totalLength)
		                    break;
                    }
                }

                if (totalString[i] == '\n')
                    lineNumber++;

                if (isWideComment)
                    continue;

                if (totalString[i] == '\n' || totalString[i] == ';')
                {
                    yield return new Line(bodyBuilder.ToString().Trim(), lineNumber);

                    bodyBuilder.Length = 0; //missing .Clear() method in .NET 3.5

                    continue;
                }

                bodyBuilder.Append(totalString[i]);
            }

            lineNumber++;
            yield return new Line(bodyBuilder.ToString().Trim(), lineNumber);
        }

        public IEnumerable<Token> TokenizeMarkup(IEnumerable<Line> lines)
        {
            List<Token> tokens = new List<Token>();

            bool isQuotationLiteral = false;
            bool isEndToken = false;
            bool isLiteralCharacter = false;

            int index = -1;
            

            foreach (Line line in lines)
            {
                int lastColumnNumber = 0;
                int columnNumber = -1;
                string currentToken = string.Empty;

                foreach (char character in line.Text)
                {
                    columnNumber++;
                    index++;

                    if (isLiteralCharacter)
                    {
                        currentToken += DetermineCharLiteral(character, columnNumber, line);
                        isLiteralCharacter = false;
                        continue;
                    }
                
                    switch (character)
                    {
                        case '\\':
                            isLiteralCharacter = true;
                            continue;
                        case '\"':
                            if (isQuotationLiteral)
                            {
                                isEndToken = true;
                            }
                            else
                            {
                                isQuotationLiteral = true;
                                continue;
                            }
                            break;
                        case ' ':
                            if (!isQuotationLiteral)
                                isEndToken = true;
                            break;
                        case '=':
                            isEndToken = true;
                            break;
                    }

                    if (isEndToken)
                    {
                        if (isQuotationLiteral)
                        {
                            if (currentToken.StartsWith("r:"))
                            {
                                tokens.Add(new Token(TokenType.LiteralRegex, line)
                                {
                                    ColumnNumber = lastColumnNumber,
                                    Value = currentToken.Substring(2),
                                });
                            }
                            else
                            {
                                tokens.Add(new Token(TokenType.LiteralString, line)
                                {
                                    ColumnNumber = lastColumnNumber,
                                    Value = currentToken,
                                });
                            }

                            isQuotationLiteral = false;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(currentToken.Trim()))
                                tokens.Add(DetermineToken(currentToken, lastColumnNumber, line));
                        }

                        lastColumnNumber = columnNumber;
                        currentToken = string.Empty;
                        isEndToken = false;
                    }
                    else
                    {
                        if (currentToken == "")
                            lastColumnNumber = columnNumber;

                        currentToken = currentToken + character;
                    }


                    if (!isQuotationLiteral && character == '=')
                    {
                        tokens.Add(new Token(TokenType.OperationAssign, line)
                        {
                            ColumnNumber = columnNumber,
                        });
                    }
                }

                if (!string.IsNullOrEmpty(currentToken.Trim()))
                    tokens.Add(DetermineToken(currentToken, lastColumnNumber, line));

                tokens.Add(new Token(TokenType.EndSequence, line)
                {
                    ColumnNumber = ++columnNumber
                });
            }



            return tokens;
        }

        protected Token DetermineToken(string currentToken, int columnNumber, Line line)
        {
            if (currentToken.StartsWith("#"))
            {
                return new Token(TokenType.Directive, line)
                {
                    ColumnNumber = columnNumber,
                    Value = currentToken.Substring(1)
                };
            }
            else
            {
                return new Token(TokenType.LiteralString, line)
                {
                    ColumnNumber = columnNumber,
                    Value = currentToken
                }; 
            }
        }

        protected char DetermineCharLiteral(char character, int columnNumber, Line line)
        {
            switch (character)
            {
                case 'n':
                    return '\n';
                case 'r':
                    return '\r';
                case '\\':
                    return '\\';
            }

            throw new LexerException($"Invalid character escape sequence \"\\{character}\"", columnNumber, line.LineNumber, line.Text);
        }
    }

    public class LexerException : Exception
    {
        public Token Token { get; protected set; }

        protected string _message;
        public override string Message => _message;

        public LexerException(string message, int columnNumber, int lineNumber, string line)
        {
            _message = CreateMessage(message, columnNumber, lineNumber, line);
        }

        public static string CreateMessage(string message, int columnNumber, int lineNumber, string line)
        {
            string totalMessage = $"{message}\r\nat line {lineNumber}, column {columnNumber}\r\n{line.Insert(columnNumber, " ")}\r\n";

            totalMessage += "^".PadLeft(columnNumber + 1, ' ');

            return totalMessage;
        }
    }
}
