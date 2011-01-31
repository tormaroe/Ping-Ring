using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using PingLang.Core.Parsing;
using System.Text;

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
            AddNode(node);
            _dot.AppendLine("}");

            return _dot.ToString();
        }

        private void RewriteTree(AST node)
        {
            node.Token.Text = string.Format("\"{0}: {1}\"", _id.Next(), node.Token.Text.Replace('"', '\''));
            node.Children.ForEach(c => RewriteTree(c));
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

    public class IntGenerator
    {
        private int i;

        public int Next()
        {
            return ++i;
        }
    }
}
