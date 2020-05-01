using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TelloDroneController.src
{
    class DroneStatus
    {
        public void Parse(Dictionary<string, string> StatusValues)
        {
            Battery = int.Parse(StatusValues["bat"]);
            HeightCM = int.Parse(StatusValues["h"]);
            SpeedX = int.Parse(StatusValues["vgx"]);
            HighestTemperature = int.Parse(StatusValues["temph"]);
        }

        public int Battery;
        public int HeightCM;
        public int SpeedX;
        public int HighestTemperature;
    }
}
