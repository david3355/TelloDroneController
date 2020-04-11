using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUdpSocket;

namespace TelloDroneController.src
{
    class TelloClient
    {
        public TelloClient(string DroneIp)
        {
            commandQueue = new Queue<string>();
            commanderSender = new UDPSocket();
            droneStatusReceiver = new UDPSocket();
            droneStatusReceiver.Server("0.0.0.0", 8890);
            droneStatusReceiver.Receive(ProcessDroneStatus);
            commanderSender.Server("0.0.0.0", 9000);
            commanderSender.Receive(ProcessCommandResponse);
            commanderSender.Client(DroneIp, 8889);

        }

        private UDPSocket commanderSender;
        private UDPSocket droneStatusReceiver;
        private event TelloResponseCallback receiveEvent;
        private event DroneStatusHandler receiveStatusEvent;
        private Queue<string> commandQueue;

        public void SendCommand(Command DroneCommand, string Parameter = "")
        {
            string command = Commander.GetCommand(DroneCommand, Parameter);
            commanderSender.Send(command);
            lock (commandQueue)
            {
                commandQueue.Enqueue(command);
            }
        }

        public void AddReceiver(TelloResponseCallback Receiver)
        {
            receiveEvent += Receiver;
        }

        public void RemoveReceiver(TelloResponseCallback Receiver)
        {
            receiveEvent -= Receiver;
        }

        public void AddStatusReceiver(DroneStatusHandler StatusReceiver)
        {
            receiveStatusEvent += StatusReceiver;
        }

        public void RemoveStatusReceiver(DroneStatusHandler StatusReceiver)
        {
            receiveStatusEvent -= StatusReceiver;
        }

        private void ProcessCommandResponse(string SenderHostAddress, int SenderPort, string Response)
        {
            if (receiveEvent != null)
            {
                string lastCommand = String.Empty;
                lock (commandQueue)
                {
                    if (commandQueue.Count != 0) lastCommand = commandQueue.Dequeue();
                }
                receiveEvent(SenderHostAddress, SenderPort, lastCommand, Response);
            }
        }

        private void ProcessDroneStatus(string SenderHostAddress, int SenderPort, string Response)
        {
            Dictionary<string, string> statusValues = new Dictionary<string, string>();
            string[] telloStatus = Response.Split(';');
            foreach (string statusKvPair in telloStatus)
            {
                string[] pair = statusKvPair.Split(':');
                if (pair.Length == 2) statusValues.Add(pair[0], pair[1]);
            }
            if (receiveStatusEvent != null) receiveStatusEvent(statusValues);
        }



    }
}
