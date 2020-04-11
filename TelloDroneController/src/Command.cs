using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TelloDroneController.src
{
    public enum Command
    {
        COMMAND,
        TAKEOFF,
        LAND,
        EMERGENCY,
        FORWARD,
        BACK,
        LEFT,
        RIGHT,
        UP,
        DOWN,
        ROTATE_CLOCKWISE,
        ROTATE_COUNTER_CLOCKWISE
    }


    public static class Commander
    {
        public static string GetCommand(Command DroneCommand, string Parameter = "")
        {
            switch (DroneCommand)
            {
                case Command.COMMAND: return "command";
                case Command.TAKEOFF: return "takeoff";
                case Command.LAND: return "land";
                case Command.EMERGENCY: return "emergency";
                case Command.FORWARD: return String.Format("forward {0}", Parameter); // 20 - 500
                case Command.BACK: return String.Format("back {0}", Parameter); // 20 - 500
                case Command.LEFT: return String.Format("left {0}", Parameter); // 20 - 500
                case Command.RIGHT: return String.Format("right {0}", Parameter); // 20 - 500
                case Command.UP: return String.Format("up {0}", Parameter); // 20 - 500
                case Command.DOWN: return String.Format("down {0}", Parameter); // 20 - 500
                case Command.ROTATE_COUNTER_CLOCKWISE: return String.Format("ccw {0}", Parameter); // 1 - 3600
                case Command.ROTATE_CLOCKWISE: return String.Format("cw {0}", Parameter); // 1 - 3600
                default: return String.Empty;
            }
        }

    }
}
