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

        /// <summary>
        /// program : ACTOR_ID* ; // match zero or more actors
        /// </summary>
        protected override void Program()
        {
            do {
               Actor();
            } while(LT(1).Type != Tokens.EOF);
        }

        /// <summary>
        /// ACTOR : T* ID BODY* . ; // ACTOR_ID postfixed ':'
        /// </summary>
        private void Actor()
        {
            ConsumeLeadingTerminators();

            Match(Tokens.ID);
            
            while (LT(1).Type != Tokens.ACTOR_END)
                Body();
            
            Match(Tokens.ACTOR_END);
        }

        /// <summary>
        /// BODY : T* LISTEN|COUNT|WHEN_WITH_BODY ;
        /// </summary>
        private void Body()
        {
            ConsumeLeadingTerminators();

            switch (LT(1).Type)
            {
                case Tokens.LISTEN: Listen(); break;
                case Tokens.COUNT: Count(); break;
                case Tokens.WHEN: When(); break;
                default:
                    throw new Exception("Unexpected actor body " + LT(1));
            }
        }

        /// <summary>
        /// LISTEN : 'listen on' INT ;
        /// </summary>
        private void Listen()
        {
            Match(Tokens.LISTEN);
            Match(Tokens.INT);
        }

        /// <summary>
        /// COUNT : 'count every' INT 'second' ;
        /// </summary>
        private void Count()
        {
            Match(Tokens.COUNT);
            Match(Tokens.INT);
            MatchUnitId();
        }

        /// <summary>
        /// WHEN_WITH_BODY : WHEN EVENT EVENT_BODY ;
        /// </summary>
        private void When()
        {
            Match(Tokens.WHEN);
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
                    Match(Tokens.GT);
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
        }

        /// <summary>
        /// PRINT : (INT|STRING|MESSAGE)* ;
        /// </summary>
        private void Print()
        {
            Match(Tokens.PRINT);
            do
            {
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
            Match(Tokens.INT);
            Match(Tokens.ID);
        }

        /// <summary>
        /// SEND STRING TO_PORT INT
        /// </summary>
        private void Send()
        {
            Match(Tokens.SEND);
            Match(Tokens.STRING);
            Match(Tokens.TO_PORT);
            Match(Tokens.INT);
        }
    }
}
