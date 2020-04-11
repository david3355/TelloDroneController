using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonUdpSocket
{
    public delegate void UdpResponseCallback(string SenderHostAddress, int SenderPort, string Message);
}
