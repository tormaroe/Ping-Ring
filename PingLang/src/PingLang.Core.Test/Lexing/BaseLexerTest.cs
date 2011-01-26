using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PingLang.Core.Lexing;

namespace PingLang.Core.Test
{
    [TestFixture]
    public class BaseLexerTest
    {
        private Lexer lexer;
        private TokenizeThis _attributes;

        public List<Token> tokens { get { return lexer.Tokens; } }

        [SetUp]
        public void SetUp()
        {
            GetTextToTokenize();
            lexer = new Lexer(Tokens.All);
            lexer.Tokenize(_attributes.Input);

            Console.WriteLine("Tokens for {0}", _attributes.Input);
            tokens.ForEach(tok => Console.WriteLine(tok));
        }

        private void GetTextToTokenize()
        {
            try
            {
                _attributes = this.GetType()
                    .GetCustomAttributes(typeof(TokenizeThis), false)
                    .First() as TokenizeThis;
            }
            catch (Exception)
            {
                Assert.Fail("Test class {0} does not specify a TokenizeThis attribute!", this.GetType().Name);
            }
        }

        protected void ExpectTokens(params int[] tokenTypes)
        {
            tokens.Count.ShouldEqual(tokenTypes.Length + 1);

            for (int i = 0; i < tokenTypes.Length; i++)
                Tokens.TokenNames[tokens[i].Type].ShouldEqual(Tokens.TokenNames[tokenTypes[i]]);

            tokens.Last().Type.ShouldEqual(Tokens.EOF);
        }

        protected void ExpectTokens(params string[] tokenContent)
        {
            tokens.Count.ShouldEqual(tokenContent.Length + 1);

            for (int i = 0; i < tokenContent.Length; i++)
                tokens[i].Text.ShouldEqual(tokenContent[i]);
        }
    }
}
