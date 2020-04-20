using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;

namespace TelloDroneController.src
{
    class AvailabilityChecker
    {
        public static bool IsAddressAvailable(string Address)
        {
            Ping ping = new Ping();
            PingReply pingReply = ping.Send(Address);

            if (pingReply.Status == IPStatus.Success)
            {
                return true;
            }
            return false;
        }
    }
}
