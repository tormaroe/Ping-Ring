-module(ring_server).
-export([start/1, start/4]).

start(Args) -> 
  % when started from command line args are collected in a list
  [A1, A2, A3, A4] = Args,
  start(A1, A2, A3, A4).

start(ThisPort, OtherPort, MaxDelay, InitialPing) 
when is_list(ThisPort) -> 
  % when started from command line some conversion is needed
  start(
    as_int(ThisPort), 
    as_int(OtherPort), 
    as_int(MaxDelay), 
    InitialPing);

start(ThisPort, OtherPort, MaxDelay, InitialPing) ->
  io:format("** Erlang Ring Server (~p)~n", [ThisPort]),
  Message = lists:concat(["Ping from ", ThisPort]),
  Pinger = spawn(fun() -> ping_sender(OtherPort, Message) end),
  case InitialPing of 
    "true" -> Pinger ! ping; 
    _ -> no_action_needed 
  end,
  Watcher = spawn(fun() -> ping_watcher(Pinger, MaxDelay, 0) end),
  spawn(fun() -> ping_listener(ThisPort, Pinger, Watcher) end),
  timer:sleep(infinity). % let the actors do the job forever

as_int(String) -> % simplified string to integer conversion
  {Int, _Rest} = string:to_integer(String),
  Int.

%% --- Actor 1 : Ping listener ---

ping_listener(ThisPort, Pinger, Watcher) ->
  {ok, LSock} = gen_tcp:listen(ThisPort, [binary, {packet, 0}, {active, false}]),
  ping_listener_loop(LSock, Pinger, Watcher).
  
ping_listener_loop(LSock, Pinger, Watcher) ->
  {ok, Sock} = gen_tcp:accept(LSock),
  {ok, Bin} = do_recv(Sock, []),
  ok = gen_tcp:close(Sock),
  process_incoming_ping(Bin, Pinger, Watcher),
  ping_listener_loop(LSock, Pinger, Watcher).
  
do_recv(Sock, Bs) ->
  case gen_tcp:recv(Sock, 0) of
    {ok, B} -> do_recv(Sock, [Bs, B]);
    {error, closed} -> {ok, list_to_binary(Bs)}
  end.

process_incoming_ping(Message, Pinger, Watcher) ->
  io:format("Received ~p~n", [Message]),
  Watcher ! ping, % tell the watcher about it 
  Pinger ! ping. % and the pinger, so he can forward it

%% --- Actor 2 : Ping sender ---

ping_sender(Port, Message) ->
  receive ping ->
      timer:sleep(1000),
      case gen_tcp:connect("localhost", Port, [binary, {packet, 0}]) of
        {ok, Sock} ->
          ok = gen_tcp:send(Sock, Message),         
          ok = gen_tcp:close(Sock);
        {error, _} ->
          io:format("*** Failed sending ping~n")
      end,
      ping_sender(Port, Message) % loop to wait for more ping requests
  end.

%% --- Actor 3 : Missing pings watcher

ping_watcher(Pinger, MaxDelay, DelayCount) ->
  receive ping -> 
      ping_watcher(Pinger, MaxDelay, 0) % all is well, watch again
  after MaxDelay * 1000 ->
      NewDelayCount = DelayCount + 1,
      io:format("*** ALERT, RING BROKEN! No ping in ~p seconds.~n", 
        [MaxDelay * NewDelayCount]),
      Pinger ! ping, % try to wake up ping ring
      ping_watcher(Pinger, MaxDelay, NewDelayCount) % watch again
  end.
