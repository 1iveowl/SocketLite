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
        public string RemoteAddress { get; internal set; }
        public string RemotePort { get; internal set; }
        public byte[] ByteData { get; internal set; }
    }
}
