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
            stream_on = false;
        }

        private UDPSocket commanderSender;
        private UDPSocket droneStatusReceiver;
        private ITelloEvents eventHandler;
        private Queue<string> commandQueue;
        private Queue<DroneResponse> responseQueue;

        private int speed;
        private const int DEFAULT_SPEED = 30;
        private const int RESPONSE_TIMEOUT_MS = 5000;
        private bool stream_on;

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

        public bool IsStreamOn
        {
            get { return stream_on; }
        }

        public bool IncreaseSpeed(int By = 5)
        {
            int newSpeed = CurrentSpeed + By;
            try
            {
                string command = TelloCommand.SetSpeed.GetCommand(newSpeed);
                DroneResponse resp = SendCommandWaitResponse(command);
                if (resp.Response == DroneResponseValue.OK)
                {
                    speed = newSpeed;
                    return true;
                }
            }
            catch (CommandIntegerParamException e)
            {}
            return false;
        }

        public bool DecreaseSpeed(int By = 5)
        {
            return IncreaseSpeed(-By);
        }

        /// <summary>
        /// Executes the given command on a separate thread (asynchronously), but thread finishes only when the response is received
        /// </summary>
        /// <param name="DroneCommand"></param>
        private void ExecuteCommandAsyncWaitResponse(string DroneCommand)
        {
            Thread executor = new Thread(new ParameterizedThreadStart(ExecuteCommandSyncWaitResponse));
            executor.Start(DroneCommand);
        }

        /// <summary>
        /// Executes the command on the caller thread (synchronously). Returns immmediately, without waiting for the response
        /// </summary>
        /// <param name="DroneCommand"></param>
        private void ExecuteCommandSyncNoWaitResponse(string DroneCommand)
        {
            SendCommand(DroneCommand);
        }

        /// <summary>
        ///  Executes the given command on the caller thread (synchronously), and blocks the thread until response is received
        /// </summary>
        /// <param name="Parameter"></param>
        private void ExecuteCommandSyncWaitResponse(object Parameter)
        {
            SendCommandWaitResponse((string)Parameter);
        }

        private void SendCommand(string DroneCommand)
        {
            commanderSender.Send(DroneCommand);
            eventHandler.CommandSent(DroneCommand, true);
        }

        private DroneResponse SendCommandWaitResponse(string DroneCommand)
        {
            lock (commandQueue)
            {
                if (commandQueue.Count > 0) return null;
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
                    return response;
                }
            }
            return null;
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
            ExecuteCommandAsyncWaitResponse(TelloCommand.Command.GetCommand());
        }

        public void TakeOff()
        {
            ExecuteCommandAsyncWaitResponse(TelloCommand.TakeOff.GetCommand());
        }

        public void Land()
        {
            ExecuteCommandSyncNoWaitResponse(TelloCommand.Land.GetCommand());
        }

        public void Emergency()
        {
            ExecuteCommandAsyncWaitResponse(TelloCommand.Emergency.GetCommand()); // Has to be sync, otherwise response cannot be processed
        }

        public bool StreamOn()
        {
            DroneResponse resp = SendCommandWaitResponse(TelloCommand.StreamOn.GetCommand());
            if (resp.Response == DroneResponseValue.OK)
            {
                stream_on = true;
                return true;
            }
            return false;
        }

        public bool StreamOff()
        {
            DroneResponse resp = SendCommandWaitResponse(TelloCommand.StreamOff.GetCommand());
            if (resp.Response == DroneResponseValue.OK)
            {
                stream_on = false;
                return true;
            }
            return false;
        }

        public void Forward(int DistanceCM)
        {
            ExecuteCommandAsyncWaitResponse(TelloCommand.Forward.GetCommand(DistanceCM));
        }

        public void Backward(int DistanceCM)
        {
            ExecuteCommandAsyncWaitResponse(TelloCommand.Back.GetCommand(DistanceCM));
        }

        public void Left(int DistanceCM)
        {
            ExecuteCommandAsyncWaitResponse(TelloCommand.Left.GetCommand(DistanceCM));
        }

        public void Right(int DistanceCM)
        {
            ExecuteCommandAsyncWaitResponse(TelloCommand.Right.GetCommand(DistanceCM));
        }

        public void Up(int DistanceCM)
        {
            ExecuteCommandAsyncWaitResponse(TelloCommand.Up.GetCommand(DistanceCM));
        }

        public void Down(int DistanceCM)
        {
            ExecuteCommandAsyncWaitResponse(TelloCommand.Down.GetCommand(DistanceCM));
        }

        public void TurnLeft(int Degree)
        {
            ExecuteCommandAsyncWaitResponse(TelloCommand.RotateCounterClockwise.GetCommand(Degree));
        }

        public void TurnRight(int Degree)
        {
            ExecuteCommandAsyncWaitResponse(TelloCommand.RotateClockwise.GetCommand(Degree));
        }

        public void Flip(FlipDirection Direction)
        {
            ExecuteCommandAsyncWaitResponse(TelloCommand.Flip.GetCommand(Direction));
        }

        public void Go(int X, int Y, int Z, int Speed)
        {
            ExecuteCommandAsyncWaitResponse(TelloCommand.Go.GetCommand(X, Y, Z, Speed));
        }

        public void Curve(int X1, int Y1, int Z1, int X2, int Y2, int Z2, int Speed)
        {
            ExecuteCommandAsyncWaitResponse(TelloCommand.Curve.GetCommand(X1, Y1, Z1, X2, Y2, Z2, Speed));
        }

        public void SetWifi(string SSID, string Password)
        {
            ExecuteCommandAsyncWaitResponse(TelloCommand.SetWifi.GetCommand(SSID, Password));
        }

        public void RemoteControl(int LeftRight, int ForwardBackward, int UpDown, int Yaw)
        {
            ExecuteCommandSyncNoWaitResponse(TelloCommand.RemoteControl.GetCommand(LeftRight, ForwardBackward, UpDown, Yaw));
        }

        public void StartRotors()
        {
            RemoteControl(-100, -100, -100, 100);
        }

        public void MakeDroneStill()
        {
            RemoteControl(0, 0, 0, 0);
        }

        public void SetSpeed(int Speed)
        {
            ExecuteCommandAsyncWaitResponse(TelloCommand.SetSpeed.GetCommand(Speed));
        }


        #endregion


        #region Queries

        private int GetInt(string QueryCommand, int DefaultValue)
        {
            DroneResponse response = SendCommandWaitResponse(QueryCommand);
            int value = DefaultValue;
            if (response != null) int.TryParse(response.Response, out value);
            return value;
        }

        private string GetString(string QueryCommand, string DefaultValue = "")
        {
            DroneResponse response = SendCommandWaitResponse(QueryCommand);
            if (response != null) return response.Response;
            return DefaultValue;
        }

        public int GetSpeed()
        {
            return GetInt(TelloCommand.Query.Speed.GetCommand(), -1);
        }

        public int GetBattery()
        {
            return GetInt(TelloCommand.Query.Battery.GetCommand(), -1);
        }

        public string GetTime()
        {
            return GetString(TelloCommand.Query.Time.GetCommand());
        }

        public string GetWifiSignalStrength()
        {
            return GetString(TelloCommand.Query.Wifi.GetCommand());
        }

        public string GetSDK()
        {
            return GetString(TelloCommand.Query.SDK.GetCommand());
        }

        public string GetSerialNumber()
        {
            return GetString(TelloCommand.Query.SerialNumber.GetCommand());
        }

        #endregion

    }
}
