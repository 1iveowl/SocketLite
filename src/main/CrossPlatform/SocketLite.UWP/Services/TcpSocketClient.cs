﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using ISocketLite.PCL.Interface;
using ISocketLite.PCL.Model;
using SocketLite.Services.Base;

namespace SocketLite.Services
{
    public class TcpSocketClient : TcpSocketBase, ITcpSocketClient
    {
        public bool IsConnected { get; private set; } = false;
        public string LocalPort => Socket.Information.LocalPort;
        public string LocalAddress => Socket.Information.LocalAddress.CanonicalName;

        public StreamSocket Socket { get; private set; }

        public Stream ReadStream => Socket.InputStream.AsStreamForRead(BufferSize);

        public Stream WriteStream => Socket.OutputStream.AsStreamForWrite(BufferSize);

        public string RemoteAddress => Socket.Information.RemoteAddress.CanonicalName;

        public int RemotePort => int.Parse(Socket.Information.RemotePort);

        public TcpSocketClient() : base(0)
        {
            Socket = new StreamSocket();
        }

        public TcpSocketClient(int bufferSize) : base(bufferSize)
        {
        }

        internal TcpSocketClient(StreamSocket nativeSocket, int bufferSize) : base(bufferSize)
        {
            Socket = nativeSocket;
        }

        public async Task ConnectAsync(
            string address, 
            string service, 
            bool secure = false, 
            CancellationToken cancellationToken = default(CancellationToken), 
            bool ignoreServerCertificateErrors = false,
            TlsProtocolVersion tlsProtocolVersion = TlsProtocolVersion.Tls12)
        {
            var hostName = new HostName(address);
            var remoteServiceName = service;

            var tlsProtocol = SocketProtectionLevel.PlainSocket;

            if (secure)
            {
                switch (tlsProtocolVersion)
                {
                    case TlsProtocolVersion.Tls10:
                        tlsProtocol = SocketProtectionLevel.Tls10;
                        break;
                    case TlsProtocolVersion.Tls11:
                        tlsProtocol = SocketProtectionLevel.Tls11;
                        break;
                    case TlsProtocolVersion.Tls12:
                        tlsProtocol = SocketProtectionLevel.Tls12;
                        break;
                    case TlsProtocolVersion.None:
                        throw new InvalidOperationException("Tls Protocol Version cannot be 'None' when establishing a secure connection. Use unencrypted WebSocket with 'None' or set TLS Protocolversion to Tls10, Tls11 or Tls12. Default when using encrypted WebSocket is Tls12");
                    default:
                        throw new ArgumentOutOfRangeException(nameof(tlsProtocolVersion), tlsProtocolVersion, null);
                }
            }

            try
            {
                await Socket.ConnectAsync(hostName, remoteServiceName, tlsProtocol);
            }
            catch (Exception ex)
            {
                if (ignoreServerCertificateErrors)
                {
                    Socket.Control.IgnorableServerCertificateErrors.Clear();

                    foreach (var ignorableError in Socket.Information.ServerCertificateErrors)
                    {
                        Socket.Control.IgnorableServerCertificateErrors.Add(ignorableError);
                    }

                    //Try again
                    try
                    {
                        await Socket.ConnectAsync(hostName, remoteServiceName, tlsProtocol);
                    }
                    catch (Exception retryEx)
                    {

                        throw retryEx;
                    }
                }
                else
                {
                    throw ex;
                }
            }
            IsConnected = true;
        }

        public void Disconnect()
        {
            IsConnected = false;
            Socket?.Dispose();
            Socket = new StreamSocket();
        }

        public void Dispose()
        {
            IsConnected = false;
        }
        
    }
}
