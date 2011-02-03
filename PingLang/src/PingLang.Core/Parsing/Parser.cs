using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PingLang.Core.Lexing;

namespace PingLang.Core.Parsing
{
    public class Parser : BaseParser
    {        
        public Parser(Lexer lexer) : base(lexer) { }

        /// <summary>
        /// program      : actor* T? EOF                         ;
        /// </summary>
        protected override void Program()
        {
            AST = new AST(new Token(Tokens.PROGRAM, ""));
            _currentNode = AST;

            UntilToken(Tokens.EOF, () =>
            {
                Actor();
                Consume_T();
            });
        }

        /// <summary>
        /// actor        : T? ID actorBody                       ;
        /// </summary>
        private void Actor()
        {
            Consume_T();
            if (TokenIs(Tokens.ID))
            {
                PreserveCurrentNode(() =>
                {
                    AddCurrentTokenAndSetAsCurrentNode();
                    ActorBody();
                });
            }
        }

        /// <summary>
        /// actorBody    : T? (listenStmt|countStmt|whenBlock)* . ; 
        /// </summary>
        private void ActorBody()
        {
            Consume_T();
            DoUntilToken(Tokens.ACTOR_END, () =>
            {
                PreserveCurrentNode(() =>
                {
                    switch (CurrentTokenType)
                    {
                        case Tokens.LISTEN: ListenStmt(); break;
                        case Tokens.COUNT: CountStmt(); break;
                        case Tokens.WHEN: WhenBlock(); break;
                        default: ThrowParseException("Unexpected actor body"); break;
                    }
                });
            });
        }

        /// <summary>
        /// listenStmt   : LISTEN INT T?                         ;
        /// </summary>
        private void ListenStmt()
        {
            AddCurrentTokenAndSetAsCurrentNode(Tokens.LISTEN);
            AddCurrentToken(Tokens.INT);
            Consume_T();
        }

        /// <summary>
        /// countStmt    : COUNT INT unit T?                     ;
        /// </summary>
        private void CountStmt()
        {
            AddCurrentTokenAndSetAsCurrentNode(Tokens.COUNT);
            AddCurrentToken(Tokens.INT);
            Unit();
            Consume_T();
        }

        /// <summary>
        /// unit         : 'second'|'seconds'                    ;
        /// </summary>
        private void Unit()
        {
            if (CurrentTextIsOneOf("second", "seconds"))
                AddCurrentToken(Tokens.ID);
            else
                ThrowParseException("Unexpected unit literal");
        }

        /// <summary>
        /// whenBlock    : WHEN eventSpec eventBody              ;
        /// </summary>
        private void WhenBlock()
        {
            AddCurrentTokenAndSetAsCurrentNode(Tokens.WHEN);
            EventSpec();
            EventBody();
        }

        /// <summary>
        /// eventSpec    : STARTING
        ///              | ERROR
        ///              | PINGED
        ///              | MESSAGE
        ///              | COUNTER GT INT                        ;
        /// </summary>
        private void EventSpec()
        {
            switch (CurrentTokenType)
            {
                case Tokens.STARTING: AddCurrentToken(); break;
                case Tokens.ERROR: AddCurrentToken(); break;
                case Tokens.PINGED: AddCurrentToken(); break;
                case Tokens.MESSAGE: AddCurrentToken(); break;
                case Tokens.COUNTER:
                    PreserveCurrentNode(() =>
                    {
                        AddCurrentTokenAndSetAsCurrentNode();
                        AddCurrentToken(Tokens.GT);
                        AddCurrentToken(Tokens.INT);
                    });
                    break;
                default:
                    ThrowParseException("Unexpected event spec");
                    break;
            }
        }

        /// <summary>
        /// eventBody    : action T*                               
        ///              | T+ (action T)+ END T+                 ;
        /// </summary>
        private void EventBody()
        {
            if (TokenIs(Tokens.T))
            {
                Consume_T();
                DoUntilToken(Tokens.END, () =>
                {
                    Action();
                    Match(Tokens.T);
                });
            }
            else
            {
                Action();
            }
            Consume_T();
        }

        /// <summary>
        /// action       : PRINT printArgs
        ///              | PING INT
        ///              | RESET
        ///              | WAIT INT unit
        ///              | SEND STRING TO_PORT INT               ;
        /// </summary>
        private void Action()
        {
            PreserveCurrentNode(() =>
            {
                switch (CurrentTokenType)
                {
                    case Tokens.PRINT:
                        AddCurrentTokenAndSetAsCurrentNode();
                        PrintArgs();
                        break;
                    case Tokens.PING:
                        AddCurrentTokenAndSetAsCurrentNode();
                        AddCurrentToken(Tokens.ID);
                        break;
                    case Tokens.RESET:
                        AddCurrentToken();
                        break;
                    case Tokens.WAIT:
                        AddCurrentTokenAndSetAsCurrentNode();
                        AddCurrentToken(Tokens.INT);
                        Unit();
                        break;
                    case Tokens.SEND:
                        AddCurrentTokenAndSetAsCurrentNode();
                        AddCurrentToken(Tokens.STRING);
                        Match(Tokens.TO_PORT);
                        AddCurrentToken(Tokens.INT);
                        break;
                    default:
                        ThrowParseException("Unexpected action");
                        break;
                }
            });
        }

        /// <summary>
        /// printArgs    : (INT|STRING|MESSAGE)*                 ;
        /// </summary>
        private void PrintArgs()
        {
            while (true)
            {
                switch (CurrentTokenType)
                {
                    case Tokens.INT: AddCurrentToken(); break;
                    case Tokens.STRING: AddCurrentToken(); break;
                    case Tokens.MESSAGE: AddCurrentToken(); break;
                    default: return;
                };
            }
        }
    }
}
