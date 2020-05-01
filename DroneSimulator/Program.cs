using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DroneSimulator.src;
using CommonUdpSocket;
using System.Timers;

namespace DroneSimulator
{
    class DroneStatus
    {
        public DroneStatus()
        {
            Battery = 100;
            Height = 0;
            Temperature = 0;
            timer = new Timer(2000);
            heightDelta = 20;
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.Start();
        }


        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Battery > 0) Battery--;
            Temperature++;
            Height += heightDelta;
            if (Height == 200 || Height == 0) heightDelta *= -1;
        }

        private Timer timer;
        private int heightDelta;

        public int Battery;
        public int Height;
        public int Temperature;
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Tello Drone Simulator";
            server = new UDPSocket();
            statusReporter = new UDPSocket();
            String host = "0.0.0.0";

            server.Server(host, COMMAND_LISTENER_PORT);
            server.Receive(ProcessMessageFromClient);
            Console.WriteLine("Drone simulator is running on [{0}:{1}]", host, COMMAND_LISTENER_PORT);

            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);

            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey(true);
            } while (keyInfo.Key != ConsoleKey.Escape);
        }

        static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            string droneStatus = String.Format(STATUS_TEMPLATE, rnd.Next(100), status.Temperature, status.Height, status.Battery);
            Console.Title = String.Format("Tello: {0}", droneStatus);
            SendDroneStatus(droneStatus);
            Console.WriteLine("Sending drone status");
        }

        private static UDPSocket server, statusReporter;
        private static int COMMAND_LISTENER_PORT = 8889;
        private static int STATUS_PORT = 8890;
        private static Timer timer = new Timer(1000);
        private static Random rnd = new Random();
        private const int COMMAND_RESPONSE_DELAY_MS = 500;
        private static DroneStatus status;
        private static string STATUS_TEMPLATE = "pitch:10;roll:30;yaw:10;vgx:{0};vgy:35;vgz:35;templ:10;temph:{1};tof:10;h:{2};bat:{3};baro:34.5;time:45;agx:20.5;agy:20.5;agz:20.5;\r\n";


        private static void ProcessMessageFromClient(string SenderHostAddress, int SenderPort, string Message)
        {
            if (status == null) status = new DroneStatus();
            if (!server.Connected)
            {
                server.Client(SenderHostAddress, SenderPort);
            }

            if (!statusReporter.Connected)
            {
                statusReporter.Client(SenderHostAddress, STATUS_PORT);
                timer.Start();
            }
            ResponseDelay(COMMAND_RESPONSE_DELAY_MS);
            string response = ProcessMessage(Message);
            server.Send(response);
            Console.WriteLine("Processing from [{1}:{2}]: {0}", Message, SenderHostAddress, SenderPort);
        }

        private static string ProcessMessage(string Message)
        {
            switch (Message)
            {
                case "speed?": return rnd.Next(1, 100).ToString();
                case "battery?": return rnd.Next(1, 100).ToString();
                case "time?": return "10:23:55";
                case "wifi?": return "-34";
                case "sdk?": return "2.0";
                case "sn?": return "23f2fsaf33fas23";
                default: return "ok";
            }
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
