using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TelloDroneController.src
{
    public delegate void TelloResponseCallback(string SenderHostAddress, int SenderPort, string LastCommand, string Message);
}
