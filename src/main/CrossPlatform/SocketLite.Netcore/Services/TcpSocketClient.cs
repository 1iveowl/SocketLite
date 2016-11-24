using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using ISocketLite.PCL.Interface;
using ISocketLite.PCL.Model;
using SocketLite.Services.Base;
using SocketException = ISocketLite.PCL.Exceptions.SocketException;
using PlatformSocketException = System.Net.Sockets.SocketException;

namespace SocketLite.Services
{
    public class TcpSocketClient : TcpSocketBase, ITcpSocketClient
    {
        private TcpClient _tcpClient;
        private SslStream _secureStream;
        private bool _ignoreCertificateErrors;

        private IPEndPoint RemoteEndpoint
        {
            get
            {
                try
                {
                    return _tcpClient.Client.RemoteEndPoint as IPEndPoint;
                }
                catch (PlatformSocketException ex)
                {
                    throw new SocketException(ex);
                }
            }
        }

        public Stream ReadStream => _secureStream != null ? _secureStream as Stream : _tcpClient.GetStream();
        public Stream WriteStream => _secureStream != null ? _secureStream as Stream : _tcpClient.GetStream();

        public string RemoteAddress => RemoteEndpoint.Address.ToString();
        public int RemotePort => RemoteEndpoint.Port;

        public TcpSocketClient() : base(0)
        {
            _tcpClient = new TcpClient();
        }

        internal TcpSocketClient(TcpClient backingClient, int bufferSize) : base(bufferSize)
        {
            _tcpClient = backingClient;
        }

        public async Task ConnectAsync(
            string address, 
            string service, 
            bool secure = false,
            CancellationToken cancellationToken = new CancellationToken(), 
            bool ignoreServerCertificateErrors = false,
            TlsProtocolVersion tlsProtocolVersion = TlsProtocolVersion.Tls12)
        {
            _ignoreCertificateErrors = ignoreServerCertificateErrors;

            var port = ServiceNames.PortForTcpServiceName(service);

            var connectTask = _tcpClient.ConnectAsync(address, port);

            var ret = new TaskCompletionSource<bool>();
            var canceller = cancellationToken.Register(() => ret.SetCanceled());

            var okOrCancelled = await Task.WhenAny(connectTask, ret.Task);

            if (okOrCancelled == ret.Task)
            {

#pragma warning disable CS4014
                // ensure we observe the connectTask's exception in case downstream consumers throw on unobserved tasks
                connectTask.ContinueWith(t => $"{t.Exception}", TaskContinuationOptions.OnlyOnFaulted);
#pragma warning restore CS4014 

                // reset the backing field.
                // depending on the state of the socket this may throw ODE which it is appropriate to ignore
                try
                {
                    Disconnect();
                }
                catch (ObjectDisposedException)
                {

                }
                return;
            }

            canceller.Dispose();

            if (secure && tlsProtocolVersion != TlsProtocolVersion.None)
            {
                SslProtocols tlsProtocol;

                switch (tlsProtocolVersion)
                {
                    case TlsProtocolVersion.Tls10:
                        tlsProtocol = SslProtocols.Tls;
                        break;
                    case TlsProtocolVersion.Tls11:
                        tlsProtocol = SslProtocols.Tls11;
                        break;
                    case TlsProtocolVersion.Tls12:
                        tlsProtocol = SslProtocols.Tls12;
                        break;
                    case TlsProtocolVersion.None:
                        tlsProtocol = SslProtocols.None;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(tlsProtocolVersion), tlsProtocolVersion, null);
                }

                var secureStream = new SslStream(_tcpClient.GetStream(), true, ValidateServerCertificate);

                try
                {
                    await secureStream.AuthenticateAsClientAsync(address, null, tlsProtocol, false);

                    _secureStream = secureStream;
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
        }

        public void Disconnect()
        {
            Dispose();
            _tcpClient = new TcpClient();
        }

        public void Dispose()
        {

#if (NETSTANDARD)
            _tcpClient?.Dispose();
#else
            _tcpClient?.Close();
#endif

            _secureStream?.Dispose();
        }

        private  bool ValidateServerCertificate(
          object sender,
          X509Certificate certificate,
          X509Chain chain,
          SslPolicyErrors sslPolicyErrors)
        {
            if (_ignoreCertificateErrors) return true;

            switch (sslPolicyErrors)
            {
                case SslPolicyErrors.RemoteCertificateNameMismatch:
                    throw new Exception($"SSL/TLS error: {SslPolicyErrors.RemoteCertificateChainErrors.ToString()}");
                case SslPolicyErrors.RemoteCertificateNotAvailable:
                    throw new Exception($"SSL/TLS error: {SslPolicyErrors.RemoteCertificateNotAvailable.ToString()}");
                case SslPolicyErrors.RemoteCertificateChainErrors:
                    throw new Exception($"SSL/TLS error: {SslPolicyErrors.RemoteCertificateChainErrors.ToString()}");
                case SslPolicyErrors.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sslPolicyErrors), sslPolicyErrors, null);
            }
            return true;
        }
    }
}
