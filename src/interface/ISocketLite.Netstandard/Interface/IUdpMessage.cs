using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISocketLite.PCL.EventArgs
{
    public interface IUdpMessage
    {
        string RemoteAddress { get; }

        string RemotePort { get; }

        byte[] ByteData { get; }
    }
}
