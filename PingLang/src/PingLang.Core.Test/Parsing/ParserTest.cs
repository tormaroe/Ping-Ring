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
    public class ParserTest
    {
        // TEst extra line breaks in block, between blocks, at end etc.
        // Test "second" literal
        // Logic to convert argument based on unit literal - should not be in parser anyway

        [Test]
        public void Tiny()
        {
            var parser = new Parser(new Lexer(Tokens.All));
            parser.ConstructTree(
                @"Ponger when pinged print ""Pong""."
                );
        }

        [Test]
        public void Test()
        {
            var parser = new Parser(new Lexer(Tokens.All));
            parser.ConstructTree(
                @"Pinger
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
        }

        [Test]
        public void Test_unexpected_count_units()
        {
            typeof(Exception).ShouldBeThrownBy(() =>
            {
                var parser = new Parser(new Lexer(Tokens.All));
                parser.ConstructTree(
                    @"Pinger
                            count every 2 months."
                    );
            },
            ex => ex.Message.Equals("Unexpected unit literal months"));
        }

        [Test]
        public void Test_unexpected_actor_body()
        {
            typeof(Exception).ShouldBeThrownBy(() =>
            {
                var parser = new Parser(new Lexer(Tokens.All));
                parser.ConstructTree(
                    @"Pinger
                            listen on port 9876
                            do something usefull
                            when message print ""Foo""."
                    );
            },
            ex => ex.Message.Equals("Unexpected actor body <'ID',\"do\">"));
        }
        
        [Test]
        public void Test_unexpected_print_argument()
        {
            typeof(Exception).ShouldBeThrownBy(() =>
            {
                var parser = new Parser(new Lexer(Tokens.All));
                parser.ConstructTree(
                    @"Pinger
                            when message print 23 ""Foo"" message foobar."
                    );
            },
            ex => ex.Message.Equals("Unexpected print argument <'ID',\"foobar\">"));
        }

        [Test]
        public void Test_unexpected_event()
        {
            typeof(Exception).ShouldBeThrownBy(() =>
            {
                var parser = new Parser(new Lexer(Tokens.All));
                parser.ConstructTree(
                    @"Pinger
                            listen on port 9876
                            when earthquake print ""Foo""."
                    );
            },
            ex => ex.Message.Equals("Unexpected event <'ID',\"earthquake\">"));
        }

        [Test]
        public void Test_unexpected_event_body()
        {
            typeof(Exception).ShouldBeThrownBy(() =>
                {
                    var parser = new Parser(new Lexer(Tokens.All));
                    parser.ConstructTree(
                        @"Pinger
                            listen on port 9876
                            when message poop ""Foo""."
                        );
                },
                ex => ex.Message.Equals("Unexpected event body <'ID',\"poop\">"));
        }
    }
}
