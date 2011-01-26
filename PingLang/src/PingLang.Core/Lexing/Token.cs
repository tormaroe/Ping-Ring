using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PingLang.Core.Lexing
{
    public class Token
    {
        public Token(int type, string text)
        {
            Text = text;
            Type = type;
        }
        public int Type { get; private set; }
        public string Text { get; private set; }

        public override string ToString()
        {
            return string.Format("<'{0}',\"{1}\">", 
                Tokens.TokenNames[Type], 
                Text.Replace("\n", "\\n").Replace("\r", "\\r"));
        }
    }
}
