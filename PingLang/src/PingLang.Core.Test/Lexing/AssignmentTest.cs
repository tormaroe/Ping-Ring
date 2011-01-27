using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PingLang.Core.Lexing;

namespace PingLang.Core.Test
{
    [TokenizeThis("Pinger.")]
    public class SimpleActor : BaseLexerTest
    {
        [Test]
        public void Test()
        {
            ExpectTokens(Tokens.ID, Tokens.ACTOR_END);
            ExpectTokens("Pinger", ".");
        }
    }

    [TokenizeThis(
                    @"Pinger
                        count every 1 second
                        when counter > 5 ping Foo
                        listen on port 9876
                        when starting ping Foo
                        when message
                            print ""Received "" message
                            ping SomeOtherPinger
                        end
                        when pinged
                            reset counter
                            wait 1 second
                            send ""Foo"" to port 1111
                        end
                        when error print ""Foo""."
        )]
    public class ComplexPinger : BaseLexerTest
    {
        [Test]  
        public void Test()
        {
            ExpectTokens(Tokens.ID, Tokens.T,
                Tokens.COUNT, Tokens.INT, Tokens.ID, Tokens.T,
                Tokens.WHEN, Tokens.COUNTER, Tokens.GT, Tokens.INT, Tokens.PING, Tokens.ID, Tokens.T,
                Tokens.LISTEN, Tokens.INT, Tokens.T,
                Tokens.WHEN, Tokens.STARTING, Tokens.PING, Tokens.ID, Tokens.T,
                Tokens.WHEN, Tokens.MESSAGE, Tokens.T,
                Tokens.PRINT, Tokens.STRING, Tokens.MESSAGE, Tokens.T,
                Tokens.PING, Tokens.ID, Tokens.T,
                Tokens.END, Tokens.T,
                Tokens.WHEN, Tokens.PINGED, Tokens.T,
                Tokens.RESET, Tokens.T,
                Tokens.WAIT, Tokens.INT, Tokens.ID, Tokens.T,
                Tokens.SEND, Tokens.STRING, Tokens.TO_PORT, Tokens.INT, Tokens.T,
                Tokens.END, Tokens.T,
                Tokens.WHEN, Tokens.ERROR, Tokens.PRINT, Tokens.STRING,
                Tokens.ACTOR_END);
            ExpectTokens("Pinger", "\r\n", 
                "count every", "1", "second", "\r\n",
                "when", "counter", ">", "5", "ping", "Foo", "\r\n",
                "listen on port", "9876", "\r\n",
                "when", "starting", "ping", "Foo", "\r\n",
                "when", "message", "\r\n",
                "print", "\"Received \"", "message", "\r\n",
                "ping", "SomeOtherPinger", "\r\n",
                "end", "\r\n",
                "when", "pinged", "\r\n", 
                "reset counter", "\r\n",
                "wait", "1", "second", "\r\n", 
                "send", "\"Foo\"", "to port", "1111", "\r\n",
                "end", "\r\n",
                "when", "error", "print", "\"Foo\"",
                ".");
        }
    }
}
