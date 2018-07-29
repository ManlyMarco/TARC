using Microsoft.VisualStudio.TestTools.UnitTesting;
using TARC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TARC.Compiler;

namespace TARC.Tests
{
    [TestClass, TestCategory("Lexer")]
    public class LexerTests
    {
        [TestMethod()]
        public void PreprocessMarkupTest()
        {
            const string input = @"japanese text=english text //TEST 1
//test 2
/* test 3 */ a //test 4
/* test 5.1
test 5.2
test 5.3 */ b
japense text = english text
""japanese text "" = ""english text """;

            Lexer lexer = new Lexer();
            var sequences = lexer.Preprocess(input);

            foreach (Line sequence in sequences)
            {
                Console.WriteLine(sequence.Text);
            }
        }

        [TestMethod()]
        public void TokenizeMarkupTest()
        {
            const string input = @"japanese text=english text
japense text = english text
""japanese text "" = ""english text ""
r:""regEX"" = sjkldfjlsdfkj";

            Lexer lexer = new Lexer();
            IEnumerable<Token> tokens = lexer.TokenizeMarkup(lexer.Preprocess(input));

            foreach (Token token in tokens)
            {
                

                Console.WriteLine(token.ToString());
            }
        }
    }
}