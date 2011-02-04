// ---- PingLang Grammar ----------------------------

program      : actor* T? EOF                         ;
actor        : T? ID actorBody                       ;
actorBody    : T? (listenStmt|countStmt|whenBlock)* . ;

listenStmt   : LISTEN INT T?                         ;
countStmt    : COUNT INT unit T?                     ;
unit         : 'second'|'seconds'                    ;

whenBlock    : WHEN eventSpec eventBody              ;
eventSpec    : STARTING
             | ERROR
             | PINGED
             | MESSAGE
             | COUNTER GT INT                        ;
eventBody    : action 
             | action T                               
             | T+ (action T)+ END T+                 ;

action       : PRINT printArgs
             | PING INT
             | RESET
             | WAIT INT unit
             | SEND STRING TO_PORT INT               ;

printArgs    : (INT|STRING|MESSAGE|COUNTER)*         ;

// ---- Terminals -----------------------------------

LISTEN       : 'listen on port'                      ;
COUNT        : 'count every'                         ;
STARTING     : 'starting'                            ;
ERROR        : 'error'                               ;
PINGED       : 'pinged'                              ;
MESSAGE      : 'message'                             ;
COUNTER      : 'counter'                             ;
GT           : '>'                                   ;
PRINT        : 'print'                               ;
PING         : 'ping'                                ;
RESET        : 'reset counter'                       ;
WAIT         : 'wait'                                ;
SEND         : 'send'                                ;
TO_PORT      : 'to port'                             ;

ID           : ('a'..'z'|'A'..'Z')+ (\w|\-)*         ;
STRING       : '\"' [any char not newline]* '\"'     ;
INT          : '0'..'9'+                             ;
T            : (\r|\n|;)+                            ;

// ---- Discarded by lexer --------------------------

WS           : ( |\t)+                               ;
COMMENT      : (\\\\|#) [any char not newline]*      ;
