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
        
        public Lexer(IEnumerable<TokenRecognizer> recognizers)
        {
            _recognizers = recognizers;           
        }

        // Will contain the result of the scan
        public List<Token> Tokens { get; private set; }

        public void Tokenize(string input)
        {
            _inputBuffer = input;
            Tokens = new List<Token>();

            while (MatchToken()); // Loop until MatchToken returns false

            // End the token stream with a special End Of File token
            Tokens.Add(new Token(PingLang.Core.Lexing.Tokens.EOF, ""));
        }

        private bool MatchToken()
        {
            foreach (var recognizer in _recognizers)
            {
                var match = recognizer.Pattern.Match(_inputBuffer);
                if (match.Success)
                {
                    if (recognizer.Output)
                        Tokens.Add(new Token(recognizer.TokenType, match.Value));

                    // Consume the matched token from input
                    _inputBuffer = _inputBuffer.Substring(match.Length);
                    return true;
                }
            }
            return false;
        }
    }
}
