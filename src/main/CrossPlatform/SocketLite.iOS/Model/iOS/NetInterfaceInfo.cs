using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SocketLite.Model.iOS
{
    internal class NetInterfaceInfo
    {
        public IPAddress Netmask;
        public IPAddress Address;
        public byte[] MacAddress;
        public ushort Index;
        public byte Type;
    }
}
