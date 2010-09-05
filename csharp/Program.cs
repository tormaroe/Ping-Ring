using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class RingServer
{
    public static void Main(string[] args)
    {
        Console.WriteLine("** C# Ring Server ({0})", args[0]);
        new RingServer // Parse arguments and start server..
        {
            ThisPort = int.Parse(args[0]),
            OtherPort = int.Parse(args[1]),
            MaxDelay = new TimeSpan(0, 0, int.Parse(args[2]))
        }.Start(bool.Parse(args[3]));
    }

    private DateTime _lastPingTime = DateTime.Now;
    public TimeSpan MaxDelay { get; set; }
    public int OtherPort { get; set; }
    public int ThisPort { get; set; }

    public void Start(bool sendStartupPing)
    {
        if (sendStartupPing)
            SendDelayedPing();
        StartMissingPingAlertThread();
        StartPingListener().Join();
    }

    private void SendDelayedPing()
    {
        new Thread(() =>
        {
            try
            {
                Thread.Sleep(1000);
                using (var tcpClient = new TcpClient("127.0.0.1", OtherPort))
                using (var streamWriter = new StreamWriter(tcpClient.GetStream()))
                    streamWriter.Write("PING from " + ThisPort);
            }
            catch (Exception ex)
            {
                Console.WriteLine("*** Failed sending ping: {0}", ex.Message);
            }
        }).Start();
    }

    private void StartMissingPingAlertThread()
    {
        new Thread(() =>
        {
            while (true)
            {
                Thread.Sleep(5000);
                var pingDelay = DateTime.Now - _lastPingTime;
                if (pingDelay > MaxDelay)
                {
                    Console.WriteLine("*** ALERT, RING BROKEN! No ping in {0} seconds.",
                        pingDelay.TotalSeconds);
                    SendDelayedPing(); // try to wake up ring
                }
            }
        }).Start();
    }

    private Thread StartPingListener()
    {
        var listenerThread = new Thread(() =>
        {
            var tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), ThisPort);
            tcpListener.Start();
            while (true)
                using (var connection = tcpListener.AcceptTcpClient())
                using (var streamReader = new StreamReader(connection.GetStream()))
                    ProcessIncomingPing(streamReader.ReadToEnd());
        });
        listenerThread.Start();
        return listenerThread;
    }

    private void ProcessIncomingPing(string message)
    {
        _lastPingTime = DateTime.Now;
        Console.WriteLine("Received {0}", message);
        SendDelayedPing();
    }
}
