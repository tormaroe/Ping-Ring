using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PingLang.Core.Lexing
{
    /// <summary>
    /// A generic Regex Table Lexer
    /// </summary>
    public class Lexer
    {
        private readonly IEnumerable<TokenRecognizer> _recognizers;
        private string _inputBuffer;
        
        public List<Token> Tokens { get; private set; }

        public Lexer(IEnumerable<TokenRecognizer> recognizers)
        {
            _recognizers = recognizers;           
        }

        public void Tokenize(string input)
        {
            _inputBuffer = input;
            Tokens = new List<Token>(); 
            bool parseInProgress = true;

            while (parseInProgress)
                parseInProgress = MatchToken();

            Tokens.Add(new Token(PingLang.Core.Lexing.Tokens.EOF, ""));
        }

        private bool MatchToken()
        {
            bool tokenMatch = false;

            foreach (var recognizer in _recognizers)
            {
                var match = recognizer.Pattern.Match(_inputBuffer);
                if (match.Success)
                {
                    if (recognizer.Output)
                        Tokens.Add(new Token(recognizer.TokenType, match.Value));

                    tokenMatch = true;
                    _inputBuffer = _inputBuffer.Substring(match.Length);
                }

                if (tokenMatch) break;
            }

            return tokenMatch;
        }
    }
}
