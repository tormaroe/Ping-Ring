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
        protected AST _currentNode;
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

        protected void Consume_T()
        {
            while (CurrentToken.Type == Tokens.T) Consume();
        }

        protected void SetRoot(Token root)
        {
            AST = new AST(root);
            _currentNode = AST;
        }

        protected Token CurrentToken
        {
            get
            {
                return _tokens[_tokenIndex];
            }
        }

        protected bool IsCurrentOneOf(params string[] args)
        {
            return args.Contains(CurrentToken.Text);
        }

        protected bool IsCurrentOneOf(params int[] args)
        {
            return args.Contains(CurrentToken.Type);
        }

        protected bool TokenIs(int tokenType)
        {
            return CurrentToken.Type == tokenType;
        }

        protected void Match(int x)
        {
            if (CurrentToken.Type == x)
                Consume();
            else
                throw new Exception(String.Format(
                    "expecting {0}; found {1}",
                    Tokens.TokenNames[x],
                    CurrentToken));
        }

        protected void ThrowParseException(string message)
        {
            throw new Exception(String.Format("{0} {1}", message, CurrentToken));
        }

        protected void AddCurrentToken()
        {
            AddCurrentToken(CurrentToken.Type);
        }

        protected void AddCurrentToken(int tokenType)
        {
            var newNode = new AST(CurrentToken);
            _currentNode.Children.Add(newNode);            
            Match(tokenType);
        }

        protected void AddCurrentTokenAndSetAsCurrentNode()
        {
            AddCurrentTokenAndSetAsCurrentNode(CurrentToken.Type);
        }

        protected void AddCurrentTokenAndSetAsCurrentNode(int tokenType)
        {
            var newNode = new AST(CurrentToken);
            _currentNode.Children.Add(newNode);
            _currentNode = newNode;
            Match(tokenType);
        }

        protected void PreserveCurrentNode(Action a)
        {
            var tempNode = _currentNode;
            a.Invoke();
            _currentNode = tempNode;
        }

        /// <summary>
        /// Used to eat one or more tokens (+) until specified token type reached
        /// </summary>
        protected void DoUntilToken(int tokenType, Action block)
        {
            block();
            UntilToken(tokenType, block);
        }

        /// <summary>
        /// Used to eat zero or more tokens (*) until specified token type reached
        /// </summary>
        protected void UntilToken(int tokenType, Action block)
        {
            while (CurrentToken.Type != tokenType)
                block();

            Consume();
        }
    }
}
