using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Threading;
using ISocketLite.PCL.EventArgs;
using ISocketLite.PCL.Interface;
using SocketLite.Model;

using SocketException = ISocketLite.PCL.Exceptions.SocketException;

namespace SocketLite.Services.Base
{
    public abstract class UdpSocketBase : UdpSendBase
    {

        #region Obsolete
        private readonly ISubject<IUdpMessage> _messageSubject = new Subject<IUdpMessage>();

        [Obsolete("Deprecated, please use CreateObservableListener instead")]
        public IObservable<IUdpMessage> ObservableMessages => _messageSubject.AsObservable();

        #endregion

        private CancellationTokenSource _cancellationTokenSource;

        private readonly IDictionary<string, bool> _multicastMemberships = new Dictionary<string, bool>();

        public IEnumerable<string> MulticastMemberShips => _multicastMemberships.Where(m => m.Value.Equals(true)).Select(m => m.Key);

        public bool IsMulticastActive { get; private set; } = false;

        public bool IsUnicastActive { get; private set; } = false;

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
            IsMulticastActive = false;
            IsUnicastActive = false;
            _multicastMemberships.Clear();
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
        
        protected IPEndPoint UdpClientInitialize(
            ICommunicationInterface communicationInterface, 
            int port, 
            bool allowMultipleBindToSamePort,
            bool isUdpMultiCast = false,
            string mcastAddress = null)
        {
            var ipLanAddress = (communicationInterface as CommunicationsInterface)?.NativeIpAddress ?? IPAddress.Any;

            var ipEndPoint = new IPEndPoint(ipLanAddress, port);

            try
            {
                if (isUdpMultiCast)
                {
                    MulticastInitializer(ipEndPoint, ipLanAddress, mcastAddress, allowMultipleBindToSamePort);
                }
                else
                {
                    UnicastInitializer(ipEndPoint, ipLanAddress, allowMultipleBindToSamePort);
                }

            }
            catch (Exception e)
            {
                // ReSharper disable once PossibleIntendedRethrow
                throw e;
            }

            return ipEndPoint;
        }

        private void MulticastInitializer(IPEndPoint ipEndPoint, IPAddress ipLanIpAddress, string mcastAddress, bool allowMultipleBindToSamePort)
        {
            try
            {
                BackingUdpClient = new UdpClient(ipEndPoint.Port, AddressFamily.InterNetwork)
                {
                    EnableBroadcast = true,
                };

                if (allowMultipleBindToSamePort) SetAllowMultipleBindToSamePort(ipLanIpAddress);

                //SetMulticastInterface(ipEndPoint.Address);

                //BackingUdpClient.Client.Bind(ipEndPoint);

                IsMulticastActive = true;
                
                MulticastAddMembership(ipEndPoint.Address.ToString(), mcastAddress);
            }
            catch (Exception e)
            {
                // ReSharper disable once PossibleIntendedRethrow
                throw e;
            }
            
            UpdClientManager.MulticastPort = ipEndPoint.Port;
        }

        private void ConvertUnicastToMulticast(IPAddress ipAddress)
        {
            BackingUdpClient.EnableBroadcast = true;

            SetMulticastInterface(ipAddress);

            IsMulticastActive = true;
        }

        private void UnicastInitializer(IPEndPoint ipEndPoint, IPAddress ipLanIpAddress, bool allowMultipleBindToSamePort)
        {

            IsUnicastActive = true;
            BackingUdpClient = new UdpClient();

            if (allowMultipleBindToSamePort) SetAllowMultipleBindToSamePort(ipLanIpAddress);

            BackingUdpClient.Client.Bind(ipEndPoint);

        }

        public void MulticastAddMembership(string ipLan, string mcastAddress)
        {
            if (!IsMulticastActive)
            {
                ConvertUnicastToMulticast(IPAddress.Parse(ipLan));
            }

            MulticastAddMembership(IPAddress.Parse(ipLan), IPAddress.Parse(mcastAddress));
        }

        private void MulticastAddMembership(IPAddress ipAddress, IPAddress mcastAddress)
        {
            if (!IsMulticastActive) throw new ArgumentException("Multicast interface must be initialized before adding multicast memberships");

            if (_multicastMemberships.ContainsKey(mcastAddress.ToString()))
            {
                if (_multicastMemberships[mcastAddress.ToString()].Equals(true))
                {
                    // The membership has already been added - do nothing
                    return;
                }
            }

            BackingUdpClient.JoinMulticastGroup(mcastAddress, ipAddress);
            
            _multicastMemberships.Add(mcastAddress.ToString(), true);
        }

        public void MulticastDropMembership(string ipLan, string mcastAddress)
        {
            if (!IsMulticastActive) throw new ArgumentException("Multicast interface must be initialized before dropping multicast memberships");

            if (!_multicastMemberships.ContainsKey(mcastAddress)) return;

            if (!_multicastMemberships[mcastAddress].Equals(true)) return;

            BackingUdpClient.DropMulticastGroup(IPAddress.Parse(mcastAddress));

            _multicastMemberships[mcastAddress] = false;
        }

        private void SetAllowMultipleBindToSamePort(IPAddress ipLanAddress)
        {

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                BackingUdpClient.ExclusiveAddressUse = false;
            }

            BackingUdpClient.Client.SetSocketOption(
                SocketOptionLevel.Socket,
                SocketOptionName.ReuseAddress,
                ipLanAddress.GetAddressBytes());
        }

        private int SetMulticastInterface(IPAddress ipLan)
        {
            var nics = NetworkInterface.GetAllNetworkInterfaces();

            var firstOrDefault = nics.FirstOrDefault(n => n.GetIPProperties().UnicastAddresses.FirstOrDefault(a => Equals(a.Address, ipLan)) != null);

            if (firstOrDefault != null)
            {
                var nicIndex = firstOrDefault
                    .GetIPProperties()
                    .GetIPv4Properties()
                    .Index;

                var optionValue = IPAddress.HostToNetworkOrder(nicIndex);

                try
                {
                    BackingUdpClient.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, optionValue);
                    BackingUdpClient.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, true);
                }
                catch (Exception e)
                {
                    throw e;
                }

                return nicIndex;
            }
            else
            {
                throw new ArgumentException($"Unable to find network interface with the address: {ipLan}");
            }
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
