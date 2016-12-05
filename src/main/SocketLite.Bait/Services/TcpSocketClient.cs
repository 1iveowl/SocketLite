using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ISocketLite.PCL.Interface;
using ISocketLite.PCL.Model;
using SocketLite.Services.Base;
using static SocketLite.Helper.Helper;

namespace SocketLite.Services
{
    public class TcpSocketClient : TcpSocketBase, ITcpSocketClient
    {
        public bool IsConnected { get; } = false;
        public Stream ReadStream => null;
        public Stream WriteStream => null;
        public string RemoteAddress => null;
        public int RemotePort => 0;

        public Task ConnectAsync(string address, int port, bool secure = false)
        {
            throw new NotImplementedException(BaitNoSwitch);
        }

        public Task ConnectAsync(
            string address, 
            string service, 
            bool secure = false,
            CancellationToken cancellationToken = new CancellationToken(), 
            bool ignoreServerCertificateErrors = false,
            TlsProtocolVersion tlsProtocolVersion = TlsProtocolVersion.Tls12)
        {
            throw new NotImplementedException(BaitNoSwitch);
        }

        public void Disconnect()
        {
            throw new NotImplementedException(BaitNoSwitch);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        
    }
}
