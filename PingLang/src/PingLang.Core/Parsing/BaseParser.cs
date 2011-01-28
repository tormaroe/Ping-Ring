using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PingLang.Core.Lexing;

namespace PingLang.Core.Parsing
{
    public abstract class BaseParser
    {
        private readonly Lexer _lexer;
        private List<Token> _tokens;

        int _tokenIndex;
        private readonly Lexer _input;
        public AST AST {get; protected set;}

        public BaseParser(Lexer lexer)
        {
            _lexer = lexer;
        }

        public void ConstructTree(string source)
        {
            _lexer.Tokenize(source);
            _tokens = _lexer.Tokens.ToList();

            Program();
        }

        protected abstract void Program();

        protected void Consume()
        {
            _tokenIndex++;
        }

        protected void ConsumeLeadingTerminators()
        {
            while (LT(1).Type == Tokens.T) Consume();
        }

        protected Token LT(int i)
        {
            return _tokens[i + _tokenIndex - 1];
        }

        protected void Match(int x)
        {
            if (LT(1).Type == x)
                Consume();
            else
                throw new Exception(String.Format(
                    "expecting {0}; found {1}",
                    Tokens.TokenNames[x],
                    LT(1)));
        }

    }
}
