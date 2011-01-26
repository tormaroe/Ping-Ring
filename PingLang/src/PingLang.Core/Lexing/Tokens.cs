using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PingLang.Core.Lexing
{
    public static class Tokens
    {
        public const int EOF = -1;
        public const int WS = 0;
        public const int T = 1;
        public const int ID = 2;
        public const int END = 4;
        public const int COMMENT = 5;
        public const int STRING = 6;
        public const int ASSIGN = 7;
        public const int ACTOR_END = 9;
        public const int LISTEN = 10;
        public const int INT = 11;
        public const int WHEN = 12;
        public const int MESSAGE = 13;
        public const int PRINT = 14;
        public const int PING = 15;
        public const int PINGED = 16;
        public const int WAIT = 17;
        public const int SEND = 18;
        public const int TO_PORT = 19;
        public const int ERROR = 20;
        public const int COUNT = 21;
        public const int RESET = 22;
        public const int COUNTER = 23;
        public const int GT = 24;

        public static readonly Dictionary<int, string> TokenNames = new Dictionary<int, string>
        {
            {T, "T"}, {ID, "ID"}, {END, "END"}, {COMMENT, "COMMENT"}, {STRING, "STRING"},
            {ASSIGN, "ASSIGN"}, {ACTOR_END, "ACTOR_END"},
            {LISTEN, "LISTEN"}, {WHEN, "WHEN"}, {MESSAGE, "MESSAGE"}, {PRINT, "PRINT"},
            {INT, "INT"}, {PING, "PING"}, {PINGED, "PINGED"}, {WAIT, "WAIT"},
            {SEND, "SEND"}, {TO_PORT, "TO_PORT"}, {ERROR, "ERROR"}, {COUNT, "COUNT"},
            {RESET, "RESET"}, {COUNTER, "COUNTER"}, {GT, "GT"}, {EOF, "EOF"},
        };

        public static readonly List<TokenRecognizer> All = new List<TokenRecognizer>
        {
            new TokenRecognizer(WS, "^([ \\t])+", false), // any number of space or tab
            new TokenRecognizer(T, "^[\r\n;]+", true), // any number of \r or \n
            
            new TokenRecognizer(ACTOR_END, "^\\.", true), 
            new TokenRecognizer(LISTEN, "^listen on port", true),
            new TokenRecognizer(WHEN, "^when", true), 
            new TokenRecognizer(MESSAGE, "^message", true), 
            new TokenRecognizer(PRINT, "^print", true), 
            new TokenRecognizer(PINGED, "^pinged", true), 
            new TokenRecognizer(PING, "^ping", true), 
            new TokenRecognizer(WAIT, "^wait", true), 
            new TokenRecognizer(SEND, "^send", true), 
            new TokenRecognizer(TO_PORT, "^to port", true), 
            new TokenRecognizer(ERROR, "^error", true),             
            new TokenRecognizer(COUNT, "^count every", true),             
            new TokenRecognizer(RESET, "^reset counter", true),             
            new TokenRecognizer(COUNTER, "^counter", true),             
            new TokenRecognizer(END, "^end", true), // the keyword 'end'

            new TokenRecognizer(ASSIGN, "^=", true), // the '=' character
            new TokenRecognizer(GT, "^\\>", true), // the '=' character

            // one letter followed by zero or more word characters or hyphens
            new TokenRecognizer(ID, "^([a-zA-Z])+([\\w\\-])*", true), 

            new TokenRecognizer(COMMENT, "^\\/\\/[^\\r\\n]*", true), // a comment like this (starting with '//')
            new TokenRecognizer(COMMENT, "^#[^\\r\\n]*", true), // any line starting with '#'
             
            new TokenRecognizer(INT, "^(\\d)+", true), 
            new TokenRecognizer(STRING, "^\"[^\\r\\n\"]*\"", true), // anything between two " characters
        };
    }
}
