using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PingLang.Core.Parsing;
using PingLang.Core.Lexing;

namespace PingLang.Core.Test.Parsing
{
    [TestFixture]
    public class AST_Test
    {
        [Test]
        public void Tiny()
        {
            var parser = new Parser(new Lexer(Tokens.All));
            parser.ConstructTree(
                @"Ponger when pinged print ""Pong""."
                );
            parser.AST.Children.Count.ShouldEqual(1);
            var actor = parser.AST.Children[0];
            actor.Token.Type.ShouldEqual(Tokens.ID);
            actor.Token.Text.ShouldEqual("Ponger");

            var eventHandler = actor.Children[0];
            eventHandler.Token.Type.ShouldEqual(Tokens.WHEN);
            eventHandler.Children[0].Token.Type.ShouldEqual(Tokens.PINGED);
            eventHandler.Children[1].Token.Type.ShouldEqual(Tokens.PRINT);
            eventHandler.Children[1].Children[0].Token.Text.ShouldEqual("\"Pong\"");
        }


        [Test]
        public void Test()
        {
            var parser = new Parser(new Lexer(Tokens.All));
            parser.ConstructTree(
                @"Foo when pinged print ""Hello"".

                  Pinger
                    listen on port 9876
                    count every 1 second
                    when error print ""Foo""
                    when starting ping Foo
                    when pinged 
                        ping FooBar
                        reset counter
                        send ""Foo"" to port 1111
                    end
                    when counter > 10
                        wait 1 second
                        print ""Foo""
                    end
                    when message print ""Foo""."
                );

            parser.AST.Children.Count.ShouldEqual(2);

            var pinger = parser.AST.Children[1];
            pinger.Children.Count.ShouldEqual(7);

            pinger.Children[0].Token.Type.ShouldEqual(Tokens.LISTEN);
            pinger.Children[0].Children[0].Token.Text.ShouldEqual("9876");
            
            pinger.Children[1].Token.Type.ShouldEqual(Tokens.COUNT);
            pinger.Children[1].Children[0].Token.Text.ShouldEqual("1");
            pinger.Children[1].Children[1].Token.Text.ShouldEqual("second");

            pinger.Children[2].Token.Type.ShouldEqual(Tokens.WHEN);
            pinger.Children[2].Children[0].Token.Type.ShouldEqual(Tokens.ERROR);
            pinger.Children[2].Children[1].Token.Type.ShouldEqual(Tokens.PRINT);
            pinger.Children[2].Children[1].Children[0].Token.Type.ShouldEqual(Tokens.STRING);

            pinger.Children[3].Token.Type.ShouldEqual(Tokens.WHEN);
            pinger.Children[3].Children[0].Token.Type.ShouldEqual(Tokens.STARTING);
            pinger.Children[3].Children[1].Token.Type.ShouldEqual(Tokens.PING);
            pinger.Children[3].Children[1].Children[0].Token.Type.ShouldEqual(Tokens.ID);

            pinger.Children[4].Token.Type.ShouldEqual(Tokens.WHEN);
            pinger.Children[4].Children[0].Token.Type.ShouldEqual(Tokens.PINGED);
            pinger.Children[4].Children[1].Token.Type.ShouldEqual(Tokens.PING);
            pinger.Children[4].Children[1].Children[0].Token.Text.ShouldEqual("FooBar");
            pinger.Children[4].Children[2].Token.Type.ShouldEqual(Tokens.RESET);
            pinger.Children[4].Children[3].Token.Type.ShouldEqual(Tokens.SEND);
            pinger.Children[4].Children[3].Children[0].Token.Type.ShouldEqual(Tokens.STRING);
            pinger.Children[4].Children[3].Children[1].Token.Type.ShouldEqual(Tokens.INT);

            pinger.Children[5].Children[1].Token.Type.ShouldEqual(Tokens.WAIT);
            pinger.Children[5].Children[1].Children[0].Token.Text.ShouldEqual("1");
        }
    }
}
