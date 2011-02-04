using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PingLang.Core.Parsing;
using PingLang.Core.Lexing;

namespace PingLang.Core.Actors
{
    public class SendAction
    {
        public static SendAction Create(AST sendNode)
        {
            return new SendAction(
                Int32.Parse(sendNode.Children[1].Token.Text),
                sendNode.Children[0].Token.Text);
        }

        private readonly int _port;
        private readonly string _message;

        public SendAction(int port, string message)
        {
            _message = message;
            _port = port;
        }

        public void Execute()
        {
            Console.WriteLine("SIMULATED SEND TO {0}: {1}", _port, _message);
        }
    }
}
