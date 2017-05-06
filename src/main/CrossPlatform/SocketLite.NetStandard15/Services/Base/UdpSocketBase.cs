using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using ISocketLite.PCL.EventArgs;
using ISocketLite.PCL.Interface;
using SocketLite.Model;
using SocketException = ISocketLite.PCL.Exceptions.SocketException;

namespace SocketLite.Services.Base
{
    public abstract class UdpSocketBase : UdpSendBase
    {
        private CancellationTokenSource _cancellationTokenSource;
        private readonly ISubject<IUdpMessage> _messageSubject = new Subject<IUdpMessage>();

        [Obsolete("Deprecated, please use CreateObservableListener instead")]
        public IObservable<IUdpMessage> ObservableMessages => _messageSubject.AsObservable();

        protected IObservable<IUdpMessage> CreateObservableMessageStream(CancellationTokenSource cancelToken)
        {
            _cancellationTokenSource = cancelToken;
            var observable = Observable.Create<IUdpMessage>(
                obs =>
                {
                    var disp = Observable.While(
                            () => !cancelToken.Token.IsCancellationRequested,
                            Observable.FromAsync(BackingUdpClient.ReceiveAsync))
                        .Select(msg =>
                        {
                            var message = new UdpMessage
                            {
                                ByteData = msg.Buffer,
                                RemotePort = msg.RemoteEndPoint.Port.ToString(),
                                RemoteAddress = msg.RemoteEndPoint.Address.ToString()
                            };

                            return message;
                        }).Subscribe(
                            msg => obs.OnNext(msg),
                            ex =>
                            {
                                Cleanup();
                                var getEx = NativeSocketExceptions.Contains(ex.GetType())
                                    ? new SocketException(ex)
                                    : ex; ;
                                obs.OnError(getEx);
                            },
                            () =>
                            {
                                Cleanup();
                                cancelToken.Cancel();
                            });

                    return disp;
                });

            return observable;
        }

        protected virtual void Cleanup()
        {
            
        }

        protected UdpSocketBase()
        { }


        protected void RunMessageReceiver(CancellationToken cancelToken)
        {
            var observeUdpReceive = Observable.While(
                () => !cancelToken.IsCancellationRequested,
                Observable.FromAsync(BackingUdpClient.ReceiveAsync))
                .Select(msg =>
                {
                    var message = new UdpMessage
                    {
                        ByteData = msg.Buffer,
                        RemotePort = msg.RemoteEndPoint.Port.ToString(),
                        RemoteAddress = msg.RemoteEndPoint.Address.ToString()
                    };

                    return message;
                });

            observeUdpReceive.Subscribe(
                // Message Received Args (OnNext)
                args =>
                {
                    _messageSubject.OnNext(args);
                },
                // Exception (OnError)
                ex => throw ((NativeSocketExceptions.Contains(ex.GetType()))
                    ? new SocketException(ex)
                    : ex), 
                cancelToken);
        }

        private Exception NativeException(Exception ex)
        {
            throw NativeSocketExceptions.Contains(ex.GetType())
                ? new SocketException(ex)
                : ex;
        }

        protected IPEndPoint InitializeUdpClient(
            ICommunicationInterface communicationInterface, 
            int port, 
            bool allowMultipleBindToSamePort,
            bool isUdpMultiCast = false,
            IPAddress mcastAddress = null,
            IEnumerable<string> mcastIpv6AddressList = null)
        {
            var ipAddress = (communicationInterface as CommunicationsInterface)?.NativeIpAddress ?? IPAddress.Any;

            var ipEndPoint = new IPEndPoint(ipAddress, port);

            if (isUdpMultiCast)
            {
                BackingUdpClient = new UdpClient
                {
                    EnableBroadcast = true,
                };
            }
            else
            {
                BackingUdpClient = new UdpClient();
            }

            var ipLan = IPAddress.Parse(ipEndPoint.Address.ToString());

            var bIp = ipLan.GetAddressBytes();

            if (isUdpMultiCast)
            {
                var mcastOptionIpv4 = new MulticastOption(mcastAddress, ipLan);

                BackingUdpClient.Client.SetSocketOption(
                    SocketOptionLevel.IP,
                    SocketOptionName.AddMembership,
                    mcastOptionIpv4);

                var nics = NetworkInterface.GetAllNetworkInterfaces();

                var nicIndex = nics
                    .FirstOrDefault(n => n.GetIPProperties().UnicastAddresses.FirstOrDefault(a => Equals(a.Address, ipLan)) != null)
                    .GetIPProperties()
                    .GetIPv4Properties()
                    .Index;

                var optionValue = IPAddress.HostToNetworkOrder(nicIndex);

                try
                {
                    BackingUdpClient.Client.SetSocketOption(
                        SocketOptionLevel.IP, SocketOptionName.MulticastInterface, optionValue);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                if (mcastIpv6AddressList != null)
                {
                    foreach (var ipv6Addr in mcastIpv6AddressList)
                    {
                        var mcastOptionIpv6 = new IPv6MulticastOption(IPAddress.Parse(ipv6Addr));

                        BackingUdpClient.Client.SetSocketOption(
                            SocketOptionLevel.IPv6,
                            SocketOptionName.AddMembership,
                            mcastOptionIpv6);
                    }
                }
            }

            if (allowMultipleBindToSamePort)
            {
                try
                {
                    BackingUdpClient.ExclusiveAddressUse = false;
                }
                catch (Exception)
                {

                }
                finally
                {
                    BackingUdpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, bIp);
                }
            }

            try
            {
                BackingUdpClient.Client.Bind(ipEndPoint);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                throw new SocketException(ex);
            }

            return ipEndPoint;
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
#if (NETSTANDARD1_5)
            BackingUdpClient?.Dispose();
#else
            BackingUdpClient?.Close();
#endif
            _messageSubject?.OnCompleted();
        }
    }
}
