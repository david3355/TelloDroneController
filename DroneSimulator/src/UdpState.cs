using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace DroneSimulator.src
{
    public struct UdpState
    {
        public UdpClient u;
        public IPEndPoint e;
    }
}
