using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PingLang.Core.Lexing
{
    public class TokenRecognizer
    {
        public TokenRecognizer(int tokenType, string regexPattern, bool output)
        {
            TokenType = tokenType;
            Output = output;
            Pattern = new Regex(regexPattern, RegexOptions.Compiled);

        }
        public Regex Pattern { get; private set; }
        public bool Output { get; private set; }
        public int TokenType { get; private set; }
    }
}
