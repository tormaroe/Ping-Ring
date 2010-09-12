start csharp\RingServerCSharp.exe 4445 4446 5 true
start ruby\RingServerRuby.rb 4446 4447 5 false
start boo\RingServerBoo.exe 4447 4448 5 false
start erl -run compile file erlang\ring_server.erl -run ring_server start 4448 4445 5 false
