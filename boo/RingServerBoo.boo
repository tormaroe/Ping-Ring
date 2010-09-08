import System
import System.IO
import System.Net
import System.Net.Sockets
import System.Threading

class RingServer:
	[Property(ThisPort)] _thisPort as int
	[Property(OtherPort)] _otherPort as int
	[Property(MaxDelay)] _maxDelay = TimeSpan(0, 0, 5) # default
	_lastPingTime = DateTime.Now

	def Start(sendStartupPing):
		SendDelayedPing.BeginInvoke() if sendStartupPing
		WatchForMissingPings.BeginInvoke()
		ListenForPings.BeginInvoke()
		Thread.Sleep(20) while true # Wait forever!

	def SendDelayedPing():
		try:
			Thread.Sleep(1000)
			using tcpClient = TcpClient('127.0.0.1', OtherPort):
				using streamWriter = StreamWriter(tcpClient.GetStream()):
					streamWriter.Write("PING from ${ThisPort}")
		except ex as Exception:
			print("*** Failed sending ping: ${ex.Message}")

	def WatchForMissingPings():
		while true:
			Thread.Sleep(5000)
			pingDelay = DateTime.Now - _lastPingTime
			if pingDelay > MaxDelay:
				print("*** ALERT, RING BROKEN! " + 
						"No ping in ${pingDelay.TotalSeconds} seconds.")
				SendDelayedPing.BeginInvoke()

	def ListenForPings():
		tcpListener = TcpListener(IPAddress.Parse('127.0.0.1'), ThisPort)
		tcpListener.Start()
		while true:
			using connection = tcpListener.AcceptTcpClient():
				using streamReader = StreamReader(connection.GetStream()):
					ProcessIncomingPing(streamReader.ReadToEnd())

	def ProcessIncomingPing(message):
		_lastPingTime = DateTime.Now
		print("Received ${message}")
		SendDelayedPing.BeginInvoke()

print("** Boo Ring Server (${argv[0]})")
srv = RingServer(
		ThisPort: int.Parse(argv[0]), 
		OtherPort: int.Parse(argv[1]),
		MaxDelay: TimeSpan(0, 0, int.Parse(argv[2])))
srv.Start(bool.Parse(argv[3]))
