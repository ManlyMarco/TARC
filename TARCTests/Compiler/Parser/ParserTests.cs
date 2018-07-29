using Microsoft.VisualStudio.TestTools.UnitTesting;
using TARC.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TARC.Tests
{
    [TestClass, TestCategory("Parser")]
    public class ParserTests
    {
        [TestMethod]
        public void ParseSequenceTest()
        {
            //List<Token> tokens = new List<Token>
            //{
            //    new Token(TokenType.Directive)
            //    {
            //        ColumnNumber = 0,
            //        LineNumber = 0,
            //        Line = "#ses dfgdfg",
            //        Value = "ses"
            //    }
            //};

            Parser parser = new Parser();

            const string input = @"japanese text=english text
japense text = english text
""japanese text "" = ""english text ""
#set wew true";

            Lexer lexer = new Lexer();
            IEnumerable<Token> tokens = lexer.TokenizeMarkup(lexer.Preprocess(input));
            
            var result = parser.Parse(tokens).ToList();
        }
    }
}