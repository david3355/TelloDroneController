using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DroneSimulator.src;
using CommonUdpSocket;
using System.Timers;

namespace DroneSimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Tello Drone Simulator";
            server = new UDPSocket();
            statusReporter = new UDPSocket();
            String host = "0.0.0.0";

            server.Server(host, COMMAND_LISTENER_PORT);
            server.Receive(ProcessMessageFromClient, ProcessError);
            Console.WriteLine("Drone simulator is running on [{0}:{1}]", host, COMMAND_LISTENER_PORT);

            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            
            Console.ReadKey();
        }

        static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            string droneStatus = "pitch:10;roll:30;yaw:10;vgx:{0};vgy:35;vgz:35;templ:10;temph:{1};tof:10;h:{2};bat:{3};baro:34.5;time:45;agx:20.5;agy:20.5;agz:20.5;\r\n";
            SendDroneStatus(String.Format(droneStatus, rnd.Next(100), rnd.Next(100), rnd.Next(100), rnd.Next(100)));
            Console.WriteLine("Sending drone status");
        }

        private static UDPSocket server, statusReporter;
        private static int COMMAND_LISTENER_PORT = 8889;
        private static int STATUS_PORT = 8890;
        private static Timer timer = new Timer(1000);
        private static Random rnd = new Random();

        private static void ProcessMessageFromClient(string SenderHostAddress, int SenderPort, string Message)
        {
            if (!server.Connected )
            {
                server.Client(SenderHostAddress, SenderPort);
            }

            if (!statusReporter.Connected)
            {
                statusReporter.Client(SenderHostAddress, STATUS_PORT);
                timer.Start();
            }
            ResponseDelay(2000);
            server.Send("ok");
            Console.WriteLine("Processing from [{1}:{2}]: {0}", Message, SenderHostAddress, SenderPort);
        }

        private static void ProcessError(string ErrorMessage)
        {
            Console.WriteLine("Error while receiving: {0}", ErrorMessage);
        }

        private static void ResponseDelay(int DelayMsec)
        {
            System.Threading.Thread.Sleep(DelayMsec);
        }

        private static void SendDroneStatus(string Status)
        {
            if (statusReporter.Connected) statusReporter.Send(Status);
        }
    }
}
