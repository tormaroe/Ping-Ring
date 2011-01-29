using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PingLang.Core.Parsing;
using PingLang.Core.Lexing;

namespace PingLang.Core.Actors
{
    public static class TreeFormatter
    {
        public static string ToString(AST node)
        {
            if (node.Children.Count > 0)
            {
                var children = node.Children.Aggregate("", (acc, cn) => acc + ToString(cn));
                return string.Format(" ({0}{1})", node.Token, children);
            }
            else
                return " " + node.Token;
        }
    }
}
