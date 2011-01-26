using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PingLang.Core.Lexing;

namespace PingLang.Core.Test
{
    public class TokenizeThis : Attribute
    {
        public TokenizeThis(string input)
        {
            Input = input;
        }
        public string Input { get; private set; }
    }
}
