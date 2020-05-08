using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TelloDroneController.src
{
    public abstract class DroneException : Exception
    {
        public DroneException(string Message) : base(Message) { }
    }

    public class CommandIntegerParamException : DroneException
    {
        public CommandIntegerParamException(string BaseCommand, string ParameterName, int ParameterValue, int LowerBoundary, int UpperBoundary) :
            base(String.Format("{0} command error. Value of {1} is incorrect: {2}. Valid range: ({3} - {4})", BaseCommand, ParameterName, ParameterValue, LowerBoundary, UpperBoundary))
        {
            this.baseCommand = BaseCommand;
            this.parameterName = ParameterName;
            this.parameterValue = ParameterValue;
            this.lowerBoundary = LowerBoundary;
            this.upperBoundary = UpperBoundary;
        }

        private string baseCommand, parameterName;
        private int parameterValue, lowerBoundary, upperBoundary;

        public string BaseCommand
        {
            get { return baseCommand; }
        }

        public string ParameterName
        {
            get { return parameterName; }
        }

        public int ParameterValue
        {
            get { return parameterValue; }
        }

        public int LowerBoundary
        {
            get { return lowerBoundary; }
        }

        public int UpperBoundary
        {
            get { return upperBoundary; }
        }
    }

    public class DroneCurveException : Exception
    {
    }

    public class CommandStringParamException : DroneException
    {
        public CommandStringParamException(string BaseCommand, string ParameterName, string ParameterValue) :
            base(String.Format("{0} command error. Value of {1} cannot be empty!", BaseCommand, ParameterName, ParameterValue))
        {
            this.baseCommand = BaseCommand;
            this.parameterName = ParameterName;
            this.parameterValue = ParameterValue;
        }

        private string baseCommand, parameterName;
        private string parameterValue;

        public string BaseCommand
        {
            get { return baseCommand; }
        }

        public string ParameterName
        {
            get { return parameterName; }
        }

        public string ParameterValue
        {
            get { return parameterValue; }
        }
    }

    public class DroneResponseValue
    {
        public const string OK = "ok";
    }

    public enum FlipDirection { LEFT, RIGHT, FORWARD, BACKWARD }

    public static class TelloCommand
    {
        public static void ValidateIntegerParameter(string BaseCommand, string ParameterName, int ParameterValue, int LowerBoundary, int UpperBoundary)
        {
            if (ParameterValue < LowerBoundary || UpperBoundary < ParameterValue) throw new CommandIntegerParamException(BaseCommand, ParameterName, ParameterValue, LowerBoundary, UpperBoundary);
        }

        public static void ValidateStringParameter(string BaseCommand, string ParameterName, string ParameterValue)
        {
            if (ParameterValue == String.Empty) throw new CommandStringParamException(BaseCommand, ParameterName, ParameterValue);
        }

        public class Command { public static string GetCommand() { return "command"; } }
        public class TakeOff { public static string GetCommand() { return "takeoff"; } }
        public class Land { public static string GetCommand() { return "land"; } }
        public class Emergency { public static string GetCommand() { return "emergency"; } }
        public class StreamOn { public static string GetCommand() { return "streamon"; } }
        public class StreamOff { public static string GetCommand() { return "streamoff"; } }

        public class Forward
        {
            public static string GetCommand(int X)
            {
                string baseCmd = "forward";
                TelloCommand.ValidateIntegerParameter(baseCmd, "X", X, 20, 500);
                return String.Format("{0} {1}", baseCmd, X);
            }
        }

        public class Back
        {
            public static string GetCommand(int X)
            {
                string baseCmd = "back";
                TelloCommand.ValidateIntegerParameter(baseCmd, "X", X, 20, 500);
                return String.Format("{0} {1}", baseCmd, X);
            }
        }

        public class Left
        {
            public static string GetCommand(int X)
            {
                string baseCmd = "left";
                TelloCommand.ValidateIntegerParameter(baseCmd, "X", X, 20, 500);
                return String.Format("{0} {1}", baseCmd, X);
            }
        }

        public class Right
        {
            public static string GetCommand(int X)
            {
                string baseCmd = "right";
                TelloCommand.ValidateIntegerParameter(baseCmd, "X", X, 20, 500);
                return String.Format("{0} {1}", baseCmd, X);
            }
        }

        public class Up
        {
            public static string GetCommand(int X)
            {
                string baseCmd = "up";
                TelloCommand.ValidateIntegerParameter(baseCmd, "X", X, 20, 500);
                return String.Format("{0} {1}", baseCmd, X);
            }
        }

        public class Down
        {
            public static string GetCommand(int X)
            {
                string baseCmd = "down";
                TelloCommand.ValidateIntegerParameter(baseCmd, "X", X, 20, 500);
                return String.Format("{0} {1}", baseCmd, X);
            }
        }

        public class RotateCounterClockwise
        {
            public static string GetCommand(int X)
            {
                string baseCmd = "ccw";
                TelloCommand.ValidateIntegerParameter(baseCmd, "X", X, 1, 3600);
                return String.Format("{0} {1}", baseCmd, X);
            }
        }

        public class RotateClockwise
        {
            public static string GetCommand(int X)
            {
                string baseCmd = "cw";
                TelloCommand.ValidateIntegerParameter(baseCmd, "X", X, 1, 3600);
                return String.Format("{0} {1}", baseCmd, X);
            }
        }



        public class Flip
        {
            public static string GetCommand(FlipDirection Direction)
            {
                string baseCmd = "flip";
                string direction = String.Empty;
                switch (Direction)
                {
                    case FlipDirection.FORWARD: direction = "f"; break;
                    case FlipDirection.BACKWARD: direction = "b"; break;
                    case FlipDirection.LEFT: direction = "l"; break;
                    case FlipDirection.RIGHT: direction = "r"; break;
                }

                return String.Format("{0} {1}", baseCmd, direction);
            }
        }

        public class Go
        {
            public static string GetCommand(int X, int Y, int Z, int Speed)
            {
                string baseCmd = "go";
                TelloCommand.ValidateIntegerParameter(baseCmd, "X", X, 20, 500);
                TelloCommand.ValidateIntegerParameter(baseCmd, "Y", Y, 20, 500);
                TelloCommand.ValidateIntegerParameter(baseCmd, "Z", Z, 20, 500);
                TelloCommand.ValidateIntegerParameter(baseCmd, "Speed", Speed, 10, 100);

                return String.Format("{0} {1} {2} {3} {4}", baseCmd, X, Y, Z, Speed);
            }
        }

        public class Curve
        {
            /// <summary>
            /// Fly a curve defined by the current and two given coordinates with speed (cm/s)
            /// Arc radius must be within the range of 0.5-10 meters
            /// X/Y/Z can’t be between -20 – 20 at the same time
            /// </summary>
            /// <param name="X1"></param>
            /// <param name="Y1"></param>
            /// <param name="Z1"></param>
            /// <param name="X2"></param>
            /// <param name="Y2"></param>
            /// <param name="Z2"></param>
            /// <param name="Speed"></param>
            public static string GetCommand(int X1, int Y1, int Z1, int X2, int Y2, int Z2, int Speed)
            {
                string baseCmd = "curve";
                TelloCommand.ValidateIntegerParameter(baseCmd, "X1", X1, 20, 500);
                TelloCommand.ValidateIntegerParameter(baseCmd, "Y1", Y1, 20, 500);
                TelloCommand.ValidateIntegerParameter(baseCmd, "Z1", Z1, 0, 500);
                TelloCommand.ValidateIntegerParameter(baseCmd, "X2", X2, 20, 500);
                TelloCommand.ValidateIntegerParameter(baseCmd, "Y2", Y2, 20, 500);
                TelloCommand.ValidateIntegerParameter(baseCmd, "Z2", Z2, 0, 500);
                TelloCommand.ValidateIntegerParameter(baseCmd, "Speed", Speed, 10, 100);

                return String.Format("{0} {1} {2} {3} {4} {5} {6} {7}", baseCmd, X1, Y1, Z1, X2, Y2, Z2, Speed);
            }
        }

        public class SetWifi
        {
            public static string GetCommand(string SSID, string Password)
            {
                string baseCmd = "wifi";
                TelloCommand.ValidateStringParameter(baseCmd, "SSID", SSID);
                TelloCommand.ValidateStringParameter(baseCmd, "Password", Password);
                return String.Format("{0} {1} {2}", baseCmd, SSID, Password);
            }
        }

        public class RemoteControl
        {
            /// <summary>
            /// Set radio control via four channels
            /// </summary>
            /// <param name="A">left/right</param>
            /// <param name="B">forward/backward</param>
            /// <param name="C">up/down</param>
            /// <param name="D">yaw</param>
            public static string GetCommand(int A, int B, int C, int D)
            {
                string baseCmd = "rc";
                TelloCommand.ValidateIntegerParameter(baseCmd, "A (left/right)", A, -100, 100);
                TelloCommand.ValidateIntegerParameter(baseCmd, "B (forward/backward)", B, -100, 100);
                TelloCommand.ValidateIntegerParameter(baseCmd, "C (up/down)", C, -100, 100);
                TelloCommand.ValidateIntegerParameter(baseCmd, "D (yaw)", D, -100, 100);

                return String.Format("{0} {1} {2} {3} {4}", baseCmd, A, B, C, D);
            }
        }

        public class SetSpeed
        {
            public static string GetCommand(int X)
            {
                string baseCmd = "speed";
                TelloCommand.ValidateIntegerParameter(baseCmd, "X", X, 10, 100);
                return String.Format("{0} {1}", baseCmd, X);
            }
        }

        public class Query
        {
            public class Speed { public static string GetCommand() { return "speed?"; } }
            public class Battery { public static string GetCommand() { return "battery?"; } }
            public class Time { public static string GetCommand() { return "time?"; } }
            public class Wifi { public static string GetCommand() { return "wifi?"; } }
            public class SDK { public static string GetCommand() { return "sdk?"; } }
            public class SerialNumber { public static string GetCommand() { return "sn?"; } }
        }

    }


}
