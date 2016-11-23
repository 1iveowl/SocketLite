using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using SocketLite.Model.iOS;

namespace SocketLite.Model
{
    public partial class CommunicationsInterface
    {
        protected static IPAddress GetSubnetMask(UnicastIPAddressInformation ip)
        {
            var nativeInterfaceInfo = NetInfo.GetInterfaceInfo();
            var match = nativeInterfaceInfo.FirstOrDefault(ni => ni?.Address != null 
                                                                && ip?.Address != null 
                                                                && Equals(ni.Address, ip.Address));

            return match?.Netmask;
        }
    }
}
