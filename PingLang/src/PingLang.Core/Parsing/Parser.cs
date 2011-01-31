using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PingLang.Core.Lexing;

namespace PingLang.Core.Parsing
{
    public class Parser : BaseParser
    {        
        private static string[] _unit_literals 
            = new [] {"second", "seconds"};
        
        public Parser(Lexer lexer) : base(lexer) { }
                                                                             
        private void MatchUnitId()
        {
            if (_unit_literals.Contains(LT(1).Text))
                Match(Tokens.ID);
            else
                throw new Exception("Unexpected unit literal " + LT(1).Text);
        }

        private AST _currentNode;

        /// <summary>
        /// program : ACTOR_ID* ; // match zero or more actors
        /// </summary>
        protected override void Program()
        {
            AST = new AST(new Token(Tokens.PROGRAM, ""));
            _currentNode = AST;
            do {
               Actor();
            } while(LT(1).Type != Tokens.EOF);
        }

        private void AddCurrentTokenAndSetAsCurrentNode()
        {
            var newNode = new AST(LT(1));
            _currentNode.Children.Add(newNode);
            _currentNode = newNode;
        }
        /// <summary>
        /// ACTOR : T* ID BODY* . ; // ACTOR_ID postfixed ':'
        /// </summary>
        private void Actor()
        {
            ConsumeLeadingTerminators();

            AddCurrentTokenAndSetAsCurrentNode();
            Match(Tokens.ID);
            
            while (LT(1).Type != Tokens.ACTOR_END)
                Body();
            
            Match(Tokens.ACTOR_END);
            _currentNode = AST;
        }

        /// <summary>
        /// BODY : T* LISTEN|COUNT|WHEN_WITH_BODY ;
        /// </summary>
        private void Body()
        {
            ConsumeLeadingTerminators();
            
            PreserveCurrentNodeAfterOperation(() =>
            {
                AddCurrentTokenAndSetAsCurrentNode();
                switch (LT(1).Type)
                {
                    case Tokens.LISTEN: Listen(); break;
                    case Tokens.COUNT: Count(); break;
                    case Tokens.WHEN: When(); break;
                    default:
                        throw new Exception("Unexpected actor body " + LT(1));
                }
            });
        }

        /// <summary>
        /// LISTEN : 'listen on' INT ;
        /// </summary>
        private void Listen()
        {
            Match(Tokens.LISTEN);
            _currentNode.Children.Add(new AST(LT(1)));
            Match(Tokens.INT);
        }

        /// <summary
        /// COUNT : 'count every' INT 'second' ;
        /// </summary>
        private void Count()
        {
            Match(Tokens.COUNT);
            _currentNode.Children.Add(new AST(LT(1)));
            Match(Tokens.INT);
            _currentNode.Children.Add(new AST(LT(1)));
            MatchUnitId();
        }

        /// <summary>
        /// WHEN_WITH_BODY : WHEN EVENT EVENT_BODY ;
        /// </summary>
        private void When()
        {
            Match(Tokens.WHEN);

            var eventSpec = new AST(LT(1));
            _currentNode.Children.Add(eventSpec);

            switch (LT(1).Type)
            {
                case Tokens.ERROR:
                    Match(Tokens.ERROR);
                    Event_body();
                    break;
                case Tokens.PINGED:                    
                    Match(Tokens.PINGED);
                    Event_body();
                    break;
                case Tokens.MESSAGE:
                    Match(Tokens.MESSAGE);
                    Event_body();
                    break;
                case Tokens.COUNTER:
                    Match(Tokens.COUNTER);
                    eventSpec.Children.Add(new AST(LT(1)));
                    Match(Tokens.GT);
                    eventSpec.Children.Add(new AST(LT(1)));
                    Match(Tokens.INT);
                    Event_body();
                    break;
                case Tokens.STARTING:
                    Match(Tokens.STARTING);
                    Event_body();
                    break;
                default:
                    throw new Exception("Unexpected event " + LT(1));
            }

        }

        /// <summary>
        /// EVENT_BODY : LINE .
        ///            | LINE T
        ///            | BLOCK
        ///            ;
        /// </summary>
        private void Event_body()
        {
            if (LT(1).Type == Tokens.T)
                Event_body_Block();
            else
                Event_body_Line();            
        }

        /// <summary>
        /// BLOCK : T+ (LINE T)+ END ;
        /// </summary>
        private void Event_body_Block()
        {
            ConsumeLeadingTerminators();

            while (LT(1).Type != Tokens.END)
            {
                Event_body_Line();
                Match(Tokens.T);
            }

            Match(Tokens.END);
        }

        /// <summary>
        /// LINE : PRINT ARG+
        ///      | PING INT
        ///      | RESET
        ///      | WAIT INT ID
        ///      | SEND STRING TO_PORT INT
        ///      ;
        /// </summary>
        private void Event_body_Line()
        {
            PreserveCurrentNodeAfterOperation(() =>
            {
                AddCurrentTokenAndSetAsCurrentNode();
                switch (LT(1).Type)
                {
                    case Tokens.PRINT: Print(); break;
                    case Tokens.PING: Ping(); break;
                    case Tokens.RESET: Reset(); break;
                    case Tokens.WAIT: Wait(); break;
                    case Tokens.SEND: Send(); break;
                    default:
                        throw new Exception("Unexpected event body " + LT(1));
                }
            });            
        }

        private void PreserveCurrentNodeAfterOperation(Action a)
        {
            var tempNode = _currentNode;
            a.Invoke();
            _currentNode = tempNode;
        }

        /// <summary>
        /// PRINT : (INT|STRING|MESSAGE)* ;
        /// </summary>
        private void Print()
        {
            Match(Tokens.PRINT);
            do
            {
                _currentNode.Children.Add(new AST(LT(1)));
                switch (LT(1).Type)
                {
                    case Tokens.STRING:
                        Match(Tokens.STRING);
                        break;
                    case Tokens.MESSAGE:
                        Match(Tokens.MESSAGE);
                        break;
                    case Tokens.INT:
                        Match(Tokens.INT);
                        break;
                    default:
                        throw new Exception("Unexpected print argument " + LT(1));
                }
            } while (LT(1).Type != Tokens.T && LT(1).Type != Tokens.ACTOR_END);
        }

        /// <summary>
        /// PING : 'ping' ID ;
        /// </summary>
        private void Ping()
        {
            Match(Tokens.PING);
            _currentNode.Children.Add(new AST(LT(1)));
            Match(Tokens.ID);
        }

        private void Reset()
        {
            Match(Tokens.RESET);
        }

        /// <summary>
        /// WAIT : 'wait' INT ID ;
        /// </summary>
        private void Wait()
        {
            Match(Tokens.WAIT);
            _currentNode.Children.Add(new AST(LT(1)));
            Match(Tokens.INT);
            _currentNode.Children.Add(new AST(LT(1)));
            Match(Tokens.ID);
        }

        /// <summary>
        /// SEND STRING TO_PORT INT
        /// </summary>
        private void Send()
        {
            Match(Tokens.SEND);
            _currentNode.Children.Add(new AST(LT(1)));
            Match(Tokens.STRING);
            Match(Tokens.TO_PORT);
            _currentNode.Children.Add(new AST(LT(1)));
            Match(Tokens.INT);
        }
    }
}
