using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PingLang.Core.Lexing;

namespace PingLang.Core.Parsing
{
    public class AST
    {
        public Token Token {get; private set;}
        public List<AST> Children {get; private set;}

        public AST(Token token)
        {
            Token = token;
            Children = new List<AST>();
        }
    }
}