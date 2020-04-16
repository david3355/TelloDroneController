using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TelloDroneController.src
{
    public interface ITelloEvents
    {
        void ReceiveTelloResponse(string SenderHostAddress, int SenderPort, string LastCommand, string Message);
        void ReceiveDroneStatus(Dictionary<string, string> StatusValues);
        void TelloConnectionError(string ErrorMessage);
    }
}
