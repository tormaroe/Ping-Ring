using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PingLang.Core.Parsing;
using PingLang.Core.Lexing;

namespace PingLang.Core.Actors
{
    public class PrintAction
    {
        public static PrintAction Create(AST printNode)
        {
            var p = new PrintAction();
            printNode.Children.ForEach(node =>
            {
                switch (node.Token.Type)
                {
                    case Tokens.STRING:
                        p.AddPrint(state => Console.Write(node.Token.TextWithoutQuotes()));
                        break;
                    case Tokens.COUNTER:
                        p.AddPrint(state => Console.Write(state.Self.Counter));
                        break;
                    case Tokens.INT:
                        p.AddPrint(state => Console.Write(node.Token.Text));
                        break;

                    //TODO: add message ..
                }
            });
            return p;
        }

        private List<Action<EventState>> _printers = new List<Action<EventState>>();

        public void AddPrint(Action<EventState> printer)
        {
            _printers.Add(printer);
        }

        public void Execute(EventState state)
        {
            _printers.ForEach(p => p.Invoke(state));
            Console.WriteLine();
        }
    }
}
