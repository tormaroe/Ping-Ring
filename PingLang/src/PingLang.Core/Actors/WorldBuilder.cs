using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PingLang.Core.Parsing;
using PingLang.Core.Lexing;

namespace PingLang.Core.Actors
{
    public class WorldBuilder
    {
        private readonly World _world;
        private ActorObject _currentActor;

        public WorldBuilder(World world)
        {
            _world = world;
        }

        public void Build(AST node)
        {
            switch (node.Token.Type)
            {
                case Tokens.PROGRAM: Program(node); break;
                case Tokens.WHEN: When(node); break;
                case Tokens.COUNT: Count(node); break;
                default:
                    break;
            }
        }

        private void Program(AST node)
        {
            node.Children.ForEach(n => Actor(n));    
        }
        
        private void Actor(AST node)
        {
            Console.Write("  Adding actor " + node.Token.Text);
            _currentActor = _world.CreateActor(node.Token.Text);
            node.Children.ForEach(n => Build(n));
            Console.WriteLine("");
        }

        private void Count(AST node)
        {
            int countInMilliseconds = Int32.Parse(node.Children[0].Token.Text) * Unit(node.Children[1].Token.Text);

            _currentActor.CountEvery(countInMilliseconds);
        }

        private void When(AST node)
        {
            var eventType = node.Children.First();
            switch (eventType.Token.Type)
            {
                case Tokens.STARTING: WhenStarting(node); break;
                case Tokens.PINGED: WhenPinged(node); break;
                default:
                    break;
            }
        }

        private void WhenStarting(AST node)
        {
            Console.Write(" with starting event ");
            for (int i = 1; // Skip first, which is event type 
                i < node.Children.Count; i++)
            {
                _currentActor.OnStart.Add(GetAction(node.Children[i]));
            }
        }

        private void WhenPinged(AST node)
        {
            Console.Write(" with pinged event ");
            for (int i = 1; // Skip first, which is event type 
                i < node.Children.Count; i++)
            {
                _currentActor.OnPing.Add(GetAction(node.Children[i]));
            }
        }

        private Action<EventState> GetAction(AST line)
        {
            switch (line.Token.Type)
            {
                case Tokens.PRINT:
                    var text = line.Children[0].Token.Text;
                    text = text.Substring(1, text.Length - 2);
                    return state => Console.WriteLine(text);
                case Tokens.PING:
                    return state => state.World.Ping(line.Children[0].Token.Text);
                case Tokens.WAIT:
                    int sleepInMilliseconds = Int32.Parse(line.Children[0].Token.Text) * Unit(line.Children[1].Token.Text);
                    return state => Thread.Sleep(sleepInMilliseconds);
                default:
                    throw new Exception("Unrecognized action " + line.Token);
            }            
        }

        private int Unit(string text)
        {
            var matchKey = text.ToLower();
            switch (matchKey)
            {
                case "second":
                case "seconds":
                    return 1000;
                default:
                    throw new Exception("Unrecognized unit type " + text);
            }

        }
    }
}
