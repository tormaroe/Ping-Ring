using System;
using System.Collections.Generic;
using System.Linq;
using PingLang.Core.Parsing;
using System.Text;
using PingLang.Core.Lexing;

namespace PingLang.Editor
{
    public class DotFileCreator
    {
        private StringBuilder _dot;
        private IntGenerator _id;

        public string ToDot(AST node)
        {
            _id = new IntGenerator();
            RewriteTree(node);

            _dot = new StringBuilder();
            _dot.AppendLine("digraph { ");
            _dot.AppendLine("node [color=black, style=filled, fillcolor=wheat]");
            AddNode(node);
            _dot.AppendLine("}");

            return _dot.ToString();
        }

        private void RewriteTree(AST node)
        {
            node.Token.Text = GetDisplayText(node);
            node.Children.ForEach(c => RewriteTree(c));
        }

        private string GetDisplayText(AST node)
        {
            string tokenName = Tokens.TokenNames[node.Token.Type];
            string tokenText = node.Token.Text.Replace('"', '\'');            
            if (string.IsNullOrEmpty(tokenText) || tokenName.Equals(tokenText.ToUpper()))
                return string.Format("\"{0}: {1}\"", _id.Next(), tokenName);
            else
                return string.Format("\"{0}: {1} \\\"{2}\\\"\"", _id.Next(), tokenName, tokenText);
        }

        private void AddNode(AST node)
        {
            node.Children.ForEach(c =>
            {
                _dot.AppendFormat("{0}->{1};{2}",
                    node.Token.Text,
                    c.Token.Text,
                    Environment.NewLine);
                
                AddNode(c);
            });    
        }
    }
}