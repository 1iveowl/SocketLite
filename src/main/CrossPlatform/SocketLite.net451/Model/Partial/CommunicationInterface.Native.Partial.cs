using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using ISocketLite.PCL.Interface;

namespace SocketLite.Model
{
    public partial class CommunicationsInterface : ICommunicationInterface
    {
        protected static IPAddress GetSubnetMask(UnicastIPAddressInformation ip)
        {
            return ip.IPv4Mask;
        }
    }
}
