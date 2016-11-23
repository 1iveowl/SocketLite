using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISocketLite.PCL.EventArgs;

namespace SocketLite.Model
{
    public class UdpMessage : IUdpMessage
    {
        public string RemoteAddress { get; } = Helper.Helper.BaitNoSwitch;
        public string RemotePort { get; } = Helper.Helper.BaitNoSwitch;
        public byte[] ByteData { get; } = null;
    }
}
