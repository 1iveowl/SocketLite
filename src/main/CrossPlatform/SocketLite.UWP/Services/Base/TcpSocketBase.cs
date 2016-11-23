using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using SocketLite.Services.Base;

namespace SocketLite.Services.Base
{
    public abstract class TcpSocketBase : CommonSocketBase
    {
        protected readonly int BufferSize;

        protected TcpSocketBase(int bufferSize)
        {
            BufferSize = bufferSize;
        }
    }
}
