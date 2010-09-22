
ListeningPort = Val(ProgramParameter())
FriendPort    = Val(ProgramParameter())
AlarmDelay    = Val(ProgramParameter()) * 1000
StartPing.s   = ProgramParameter()
PingMsg.s     = "Ping from " + Str(ListeningPort)
NextPing      = 0
LastPing      = 0

Procedure PingRecvThread(Port)
  Shared LastPing, NextPing
  CreateNetworkServer(0, Port)
  Repeat
    Select NetworkServerEvent()
      Case 0
        Delay(100)
      Case #PB_NetworkEvent_Data
        Buffer.s = Space(2000)
        ReceiveNetworkData(EventClient(), @Buffer, 2000)
        PrintN(Buffer)
        LastPing = ElapsedMilliseconds()
        NextPing = LastPing + 1000
    EndSelect
  ForEver
EndProcedure

Procedure Ping(Port)
  Shared PingMsg
  C = OpenNetworkConnection("localhost", Port)
  If C
    L = StringByteLength(PingMsg) + 1
    If SendNetworkData(C, @PingMsg, L) = L
      ProcedureReturn 1
    EndIf
  EndIf
EndProcedure

Procedure PingSendThread(Port)
  Shared NextPing, ListeningPort
  Repeat
    Repeat
      Delay(100)
    Until NextPing <> 0 And ElapsedMilliseconds() > NextPing
    If Not Ping(Port)
      PrintN("Knock knock! Are you dead, port " + Str(FriendPort) + "?")
    EndIf
    NextPing = 0
  ForEver
EndProcedure

Procedure PingAlarmThread(ThreadVoid)
  Shared FriendPort, NextPing, LastPing, AlarmDelay
  Repeat
    If LastPing And ElapsedMilliseconds()-LastPing > AlarmDelay
      PrintN("Weeep! I'm so lonely... No one pinged me for quite some time.")
      Ping(FriendPort)
      LastPing = ElapsedMilliseconds()
    EndIf
    Delay(1000)
  ForEver
EndProcedure

OpenConsole()
InitNetwork()

PrintN("Hello, my name is: " + Str(ListeningPort))
ConsoleTitle(Str(ListeningPort))

Recv = CreateThread(@PingRecvThread(), ListeningPort)
Send = CreateThread(@PingSendThread(), FriendPort)
Alrm = CreateThread(@PingAlarmThread(), 0)

If StartPing
  Ping(FriendPort)
EndIf

Repeat
  Delay(50)
Until Asc(Inkey()) = 27

PrintN("Goodbye from: " + Str(ListeningPort))
Input()
