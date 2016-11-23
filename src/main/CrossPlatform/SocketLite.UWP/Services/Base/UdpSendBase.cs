using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace SocketLite.Services.Base
{
    public abstract class UdpSendBase : CommonSocketBase
    {
        protected DatagramSocket DatagramSocket;

        public string LocalPort => DatagramSocket.Information.LocalPort;

        public string LocalAddress => DatagramSocket.Information.LocalAddress.CanonicalName;

        protected UdpSendBase()
        {
            
        }

        protected void InitializeUdpSocket()
        {
            DatagramSocket = new DatagramSocket();
            //SubsribeToMessages();
        }


        public virtual async Task SendAsync(byte[] data)
        {
            await SendAsync(data, data.Length).ConfigureAwait(false);
        }

        public virtual async Task SendAsync(byte[] data, int length)
        {
            var stream = DatagramSocket.OutputStream.AsStreamForWrite();

            await stream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
            await stream.FlushAsync().ConfigureAwait(false);
        }

        public virtual async Task SendToAsync(byte[] data, string address, int port)
        {
            await SendToAsync(data, data.Length, address, port).ConfigureAwait(false);
        }

        public virtual async Task SendToAsync(byte[] data, int length, string address, int port)
        {
            var hostName = new HostName(address);
            var serviceName = port.ToString();

            var stream = (await DatagramSocket.GetOutputStreamAsync(hostName, serviceName))
                            .AsStreamForWrite();

            await stream.WriteAsync(data, 0, length).ConfigureAwait(false);
            await stream.FlushAsync().ConfigureAwait(false);
        }

        protected void ConfigureDatagramSocket(bool allowMultipleBindToSamePort)
        {
#if WINDOWS_UWP
            DatagramSocket.Control.MulticastOnly = allowMultipleBindToSamePort;
#endif

#if !WINDOWS_UWP
            if (allowMultipleBindToSamePort)
            {
                throw new ArgumentException("Multiple binding to same port is only supported by Windows 10/UWP and not WinRT");
            }
#endif
        }
    }
}
