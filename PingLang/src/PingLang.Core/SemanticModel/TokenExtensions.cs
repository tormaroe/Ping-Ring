using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PingLang.Core.Parsing;
using PingLang.Core.Lexing;

namespace PingLang.Core.Actors
{
    public static class TokenExtensions
    {
        public static string TextWithoutQuotes(this Token token)
        {
            return token.Text.Substring(1, token.Text.Length - 2);
        }
    }
}
