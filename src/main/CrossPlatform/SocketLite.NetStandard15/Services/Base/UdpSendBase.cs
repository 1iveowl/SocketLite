using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SocketLite.Extensions;

namespace SocketLite.Services.Base
{
    public abstract class UdpSendBase : CommonSocketBase
    {
        protected CancellationTokenSource MessageConcellationTokenSource;

        protected UdpClient BackingUdpClient;

        protected IPEndPoint Endpoint;

        protected UdpSendBase()
        {
            
        }

        public virtual async Task SendAsync(byte[] data)
        {
            await BackingUdpClient
                .SendAsync(data, data.Length, Endpoint)
                .WrapNativeSocketExceptions()
                .ConfigureAwait(false);
        }

        public virtual async Task SendAsync(byte[] data, int length)
        {
            await BackingUdpClient
                .SendAsync(data, length, Endpoint)
                .WrapNativeSocketExceptions()
                .ConfigureAwait(false);
        }

        public virtual async Task SendToAsync(byte[] data, string address, int port)
        {
            await BackingUdpClient
               .SendAsync(data, data.Length, address, port)
               .WrapNativeSocketExceptions()
               .ConfigureAwait(false);
        }

        public virtual async Task SendToAsync(byte[] data, int length, string address, int port)
        {
            await BackingUdpClient
                .SendAsync(data, length, address, port)
                .WrapNativeSocketExceptions()
                .ConfigureAwait(false);
        }
    }
}
