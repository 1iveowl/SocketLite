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

        protected readonly IDictionary<string, bool> MulticastMemberships = new Dictionary<string, bool>();

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
            bool allowMultipleBindToSamePort = false,
            bool isUdpMultiCast = false,
            IPAddress mcastAddress = null)
        {
            var ipLanAddress = (communicationInterface as CommunicationsInterface)?.NativeIpAddress ?? IPAddress.Any;

            var ipEndPoint = new IPEndPoint(ipLanAddress, port);

#if (NETSTANDARD1_5 || NETSTANDARD1_3)
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
#else
            var p = Environment.OSVersion.Platform;

            if (p == PlatformID.Win32NT || p == PlatformID.Win32S || p == PlatformID.Win32Windows)
#endif
            {
                UdpWindowsClient(ipLanAddress, ipEndPoint, mcastAddress, allowMultipleBindToSamePort, isUdpMultiCast);
            }
            else
            {
                if (allowMultipleBindToSamePort) throw new ArgumentException("The paramenter allowMultipleBindToSamePort is only available on Windows");
                UdpLinuxOrMacClient(ipLanAddress, ipEndPoint, mcastAddress, allowMultipleBindToSamePort, isUdpMultiCast);
            }

            return ipEndPoint;
        }

        private void UdpWindowsClient(IPAddress ipLanAddress, IPEndPoint ipEndPoint, IPAddress mcastAddress, bool allowMultipleBindToSamePort, bool isUdpMultiCast)
        {
            BackingUdpClient = new UdpClient();

            if (allowMultipleBindToSamePort) SetAllowMultipleBindToSamePort(ipLanAddress);

            BackingUdpClient.Client.Bind(ipEndPoint);

            try
            {
                if (isUdpMultiCast)
                {
                    MulticastInitializer(ipEndPoint, ipLanAddress, mcastAddress);
                }
                else
                {
                    UnicastInitializer(ipEndPoint, ipLanAddress, allowMultipleBindToSamePort);
                }

                var t = "";

            }
            catch (Exception e)
            {
                // ReSharper disable once PossibleIntendedRethrow
                throw e;
            }
        }

        private void UdpLinuxOrMacClient(IPAddress ipLanAddress, IPEndPoint ipEndPoint, IPAddress mcastAddress, bool allowMultipleBindToSamePort, bool isUdpMultiCast)

        {
            BackingUdpClient = new UdpClient(ipEndPoint.Port, AddressFamily.InterNetwork);

            try
            {
                if (isUdpMultiCast)
                {
                    MulticastInitializer(ipEndPoint, ipLanAddress, mcastAddress);
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
        }

        private void UnicastInitializer(IPEndPoint ipEndPoint, IPAddress ipLanIpAddress, bool allowMultipleBindToSamePort)
        {
            IsUnicastActive = true;
            BackingUdpClient = new UdpClient(ipEndPoint.Port, AddressFamily.InterNetwork);

            //if (allowMultipleBindToSamePort) SetAllowMultipleBindToSamePort(ipLanIpAddress);
        }

        private void MulticastInitializer(IPEndPoint ipEndPoint, IPAddress ipLanIpAddress, IPAddress mcastAddress)
        {
            IsMulticastActive = true;

            try
            {
                SetMulticastInterface(ipEndPoint.Address);

                MulticastAddMembership(ipLanIpAddress, mcastAddress);
                
            }
            catch (Exception e)
            {
                // ReSharper disable once PossibleIntendedRethrow
                throw e;
            }
        }

        protected void ConvertUnicastToMulticast(IPAddress ipAddress)
        {
            BackingUdpClient.EnableBroadcast = true;

            SetMulticastInterface(ipAddress);

            IsMulticastActive = true;
        }


        protected void MulticastAddMembership(IPAddress ipAddress, IPAddress mcastAddress)
        {
            if (!IsMulticastActive) throw new ArgumentException("Multicast interface must be initialized before adding multicast memberships");

            if (MulticastMemberships.ContainsKey(mcastAddress.ToString()))
            {
                if (MulticastMemberships[mcastAddress.ToString()].Equals(true))
                {
                    // The membership has already been added - do nothing
                    return;
                }
            }

            BackingUdpClient.JoinMulticastGroup(mcastAddress, ipAddress);
            
            MulticastMemberships.Add(mcastAddress.ToString(), true);
        }

        private void SetAllowMultipleBindToSamePort(IPAddress ipLanAddress)
        {

#if (NETSTANDARD1_5 || NETSTANDARD1_3)
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
#else
            var p = Environment.OSVersion.Platform;

            if (p == PlatformID.Win32NT || p == PlatformID.Win32S || p == PlatformID.Win32Windows)
#endif
            {
                BackingUdpClient.ExclusiveAddressUse = false;
            }

            BackingUdpClient.Client.SetSocketOption(
                SocketOptionLevel.Socket,
                SocketOptionName.ReuseAddress, true); //ipLanAddress.GetAddressBytes());
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
#if (NETSTANDARD1_5 || NETSTANDARD1_3)
            BackingUdpClient?.Dispose();
#else
            BackingUdpClient?.Close();
#endif
            _messageSubject?.OnCompleted();
        }
    }
}
