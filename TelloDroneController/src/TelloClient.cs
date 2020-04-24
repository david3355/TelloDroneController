using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUdpSocket;
using System.Threading;

namespace TelloDroneController.src
{
    public class TelloClient
    {
        public TelloClient(string DroneIp, ITelloEvents TelloEventHandler)
        {
            eventHandler = TelloEventHandler;
            commandQueue = new Queue<string>();
            responseQueue = new Queue<DroneResponse>();
            commanderSender = new UDPSocket();
            droneStatusReceiver = new UDPSocket();
            droneStatusReceiver.Server("0.0.0.0", 8890);
            droneStatusReceiver.Receive(ProcessDroneStatus);
            commanderSender.Server("0.0.0.0", 9000);
            commanderSender.Receive(ProcessCommandResponse);
            commanderSender.Client(DroneIp, 8889);
            speed = DEFAULT_SPEED;
        }

        private UDPSocket commanderSender;
        private UDPSocket droneStatusReceiver;
        private ITelloEvents eventHandler;
        private Queue<string> commandQueue;
        private Queue<DroneResponse> responseQueue;

        private int speed;
        private const int DEFAULT_SPEED = 30;
        private const int RESPONSE_TIMEOUT_MS = 5000;

        class DroneResponse
        {
            public DroneResponse(string Host, int Port, string Response)
            {
                this.Host = Host;
                this.Port = Port;
                this.Response = Response;
            }

            public string Host;
            public int Port;
            public string Response;
        }

        public int CurrentSpeed
        {
            get { return speed; }
        }

        public bool IncreaseSpeed(int By = 5)
        {
            int newSpeed = CurrentSpeed + By;
            try
            {
                string command = TelloCommand.SetSpeed.GetCommand(newSpeed);
                SendCommand(command);
                speed = newSpeed;
                return true;
            }
            catch (CommandIntegerParamException e)
            {
                return false;
            }
        }

        public bool DecreaseSpeed(int By = 5)
        {
            return IncreaseSpeed(-By);
        }

        private void ExecuteCommandSync(string DroneCommand)
        {
            Thread executor = new Thread(new ParameterizedThreadStart(AsyncCommandExecutor));
            executor.Start(DroneCommand);
        }

        private void ExecuteCommandAsync(string DroneCommand)
        {
            SendCommand(DroneCommand);
        }

        private void AsyncCommandExecutor(object Parameter)
        {
            SendCommandWaitResponse((string)Parameter);
        }

        private void SendCommand(string DroneCommand)
        {
            commanderSender.Send(DroneCommand);
            eventHandler.CommandSent(DroneCommand, true);
        }

        private void SendCommandWaitResponse(string DroneCommand)
        {
            lock (commandQueue)
            {
                if (commandQueue.Count > 0) return;
                commandQueue.Enqueue(DroneCommand);
                commanderSender.Send(DroneCommand);
                eventHandler.CommandSent(DroneCommand, false);
            }

            lock (responseQueue)
            {
                Monitor.Wait(responseQueue, RESPONSE_TIMEOUT_MS);
            }

            DroneResponse response = null;
            lock (responseQueue)
            {
                if (responseQueue.Count != 0)
                {
                    response = responseQueue.Dequeue();
                    if (eventHandler != null) eventHandler.ReceiveTelloResponse(response.Host, response.Port, DroneCommand, response.Response);
                }
            }
        }

        private void ProcessCommandResponse(string SenderHostAddress, int SenderPort, string Response)
        {
            // TODO check Response
            // If response != ok, retry?
            lock (commandQueue)
            {
                if (commandQueue.Count > 0)
                {
                    commandQueue.Dequeue();
                    lock (responseQueue)
                    {
                        responseQueue.Enqueue(new DroneResponse(SenderHostAddress, SenderPort, Response));
                        Monitor.PulseAll(responseQueue);
                    }
                }
            }
            //if (receiveEvent != null)
            //{
            //    string lastCommand = String.Empty;
            //    receiveEvent(SenderHostAddress, SenderPort, lastCommand, Response);
            //}
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
            if (eventHandler != null) eventHandler.ReceiveDroneStatus(statusValues);
        }

        #region Commands

        public void Command()
        {
            ExecuteCommandSync(TelloCommand.Command.GetCommand());
        }

        public void TakeOff()
        {
            ExecuteCommandSync(TelloCommand.TakeOff.GetCommand());
        }

        public void Land()
        {
            ExecuteCommandAsync(TelloCommand.Land.GetCommand());
        }

        public void Emergency()
        {
            ExecuteCommandAsync(TelloCommand.Emergency.GetCommand());
        }

        public void StreamOn()
        {
            ExecuteCommandSync(TelloCommand.StreamOn.GetCommand());
        }

        public void StreamOff()
        {
            ExecuteCommandSync(TelloCommand.StreamOff.GetCommand());
        }

        public void Forward(int DistanceCM)
        {
            ExecuteCommandSync(TelloCommand.Forward.GetCommand(DistanceCM));
        }

        public void Backward(int DistanceCM)
        {
            ExecuteCommandSync(TelloCommand.Back.GetCommand(DistanceCM));
        }

        public void Left(int DistanceCM)
        {
            ExecuteCommandSync(TelloCommand.Left.GetCommand(DistanceCM));
        }

        public void Right(int DistanceCM)
        {
            ExecuteCommandSync(TelloCommand.Right.GetCommand(DistanceCM));
        }

        public void Up(int DistanceCM)
        {
            ExecuteCommandSync(TelloCommand.Up.GetCommand(DistanceCM));
        }

        public void Down(int DistanceCM)
        {
            ExecuteCommandSync(TelloCommand.Down.GetCommand(DistanceCM));
        }

        public void TurnLeft(int Degree)
        {
            ExecuteCommandSync(TelloCommand.RotateCounterClockwise.GetCommand(Degree));
        }

        public void TurnRight(int Degree)
        {
            ExecuteCommandSync(TelloCommand.RotateClockwise.GetCommand(Degree));
        }

        public void Flip(FlipDirection Direction)
        {
            ExecuteCommandSync(TelloCommand.Flip.GetCommand(Direction));
        }

        public void Go(int X, int Y, int Z, int Speed)
        {
            ExecuteCommandSync(TelloCommand.Go.GetCommand(X, Y, Z, Speed));
        }

        public void Curve(int X1, int Y1, int Z1, int X2, int Y2, int Z2, int Speed)
        {
            ExecuteCommandSync(TelloCommand.Curve.GetCommand(X1, Y1, Z1, X2, Y2, Z2, Speed));
        }

        public void SetWifi(string SSID, string Password)
        {
            ExecuteCommandSync(TelloCommand.SetWifi.GetCommand(SSID, Password));
        }

        public void RemoteControl(int LeftRight, int ForwardBackward, int UpDown, int Yaw)
        {
            ExecuteCommandAsync(TelloCommand.RemoteControl.GetCommand(LeftRight, ForwardBackward, UpDown, Yaw));
        }

        public void StartRotors()
        {
            RemoteControl(-100, -100, -100, 100);
        }

        public void SetSpeed(int Speed)
        {
            ExecuteCommandSync(TelloCommand.SetSpeed.GetCommand(Speed));
        }


        #endregion

    }
}
