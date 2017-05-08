using System;
using System.Collections;
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
        private bool _isMulticastInitialized = false;
        private bool _isUnicastInitialized = false;

        private readonly IDictionary<string, bool> _multicastMemberships = new Dictionary<string, bool>();

        public IEnumerable<string> MulticastMemberShips => _multicastMemberships.Where(m => m.Value.Equals(true)).Select(m => m.Key);

        public bool IsMulticastInterfaceActive => _isMulticastInitialized;
        public bool IsUnicastInterfaceActive => _isUnicastInitialized;


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
            _isMulticastInitialized = false;
            _isUnicastInitialized = false;
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

        private void SetMulticastInterface(IPAddress ipLan)
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
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            else
            {
                throw new ArgumentException($"Unable to find network interface with the address: {ipLan}");
            }
        }

        private IPEndPoint InitializeUnicase(IPEndPoint ipEndPoint, IPAddress ipLanIpAddress, bool allowMultipleBindToSamePort)
        {

            _isUnicastInitialized = true;
            BackingUdpClient = new UdpClient();


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
                    BackingUdpClient.Client.SetSocketOption(
                        SocketOptionLevel.Socket, 
                        SocketOptionName.ReuseAddress, 
                        ipLanIpAddress.GetAddressBytes());
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


        private IPEndPoint InitializeMulticast(IPEndPoint ipEndPoint, IPAddress ipLanIpAddress, string mcastAddress, bool allowMultipleBindToSamePort)
        {
            BackingUdpClient = new UdpClient(ipEndPoint)
            {
                EnableBroadcast = true,
            };

            if (allowMultipleBindToSamePort) SetAllowMultipleBindToSamePort(ipLanIpAddress);

            _isMulticastInitialized = true;

            MulticastAddMembership(ipEndPoint.Address.ToString(), mcastAddress);

            SetMulticastInterface(ipLanIpAddress);

            //try
            //{
            //    BackingUdpClient.Client.Bind(ipEndPoint);
            //}
            //catch (System.Net.Sockets.SocketException ex)
            //{
            //    throw new SocketException(ex);
            //}

            return ipEndPoint;

        }

        private void SetAllowMultipleBindToSamePort(IPAddress ipLanAddress)
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
                BackingUdpClient.Client.SetSocketOption(
                    SocketOptionLevel.Socket, 
                    SocketOptionName.ReuseAddress, 
                    ipLanAddress.GetAddressBytes());
            }

        }

        protected IPEndPoint InitializeUdpClient(
            ICommunicationInterface communicationInterface, 
            int port, 
            bool allowMultipleBindToSamePort,
            bool isUdpMultiCast = false,
            string mcastAddress = null)
        {
            var ipLanAddress = (communicationInterface as CommunicationsInterface)?.NativeIpAddress ?? IPAddress.Any;

            var ipEndPoint = new IPEndPoint(ipLanAddress, port);

            return isUdpMultiCast 
                ? InitializeMulticast(ipEndPoint, ipLanAddress, mcastAddress, allowMultipleBindToSamePort) 
                : InitializeUnicase(ipEndPoint, ipLanAddress, allowMultipleBindToSamePort);

            //var ipLan = IPAddress.Parse(ipEndPoint.Address.ToString());

            //var bIp = ipLan.GetAddressBytes();

            //if (isUdpMultiCast)
            //{
            //    _isMulticastInitialized = true;

            //    MulticastAddMembership(ipEndPoint.Address.ToString(), mcastAddress);

            //    SetMulticastInterface(ipLan);
            //}

            //if (allowMultipleBindToSamePort)
            //{
            //    try
            //    {
            //        BackingUdpClient.ExclusiveAddressUse = false;
            //    }
            //    catch (Exception)
            //    {

            //    }
            //    finally
            //    {
            //        BackingUdpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, bIp);
            //    }
            //}

            //try
            //{
            //    BackingUdpClient.Client.Bind(ipEndPoint);
            //}
            //catch (System.Net.Sockets.SocketException ex)
            //{
            //    throw new SocketException(ex);
            //}


            //return ipEndPoint;
        }

        public void MulticastAddMembership(string ipLan, string mcastAddress)
        {
            if (!_isMulticastInitialized) throw new ArgumentException("Multicast interface must be initialized before adding multicast memberships");

            if (_multicastMemberships.ContainsKey(mcastAddress))
            {
                if (_multicastMemberships[mcastAddress].Equals(true))
                {
                    // The membership has already been added - do nothing
                    return;
                }
            }

            var mcastOptionIpv4 = new MulticastOption(IPAddress.Parse(mcastAddress), IPAddress.Parse(ipLan));

            BackingUdpClient.Client.SetSocketOption(
                SocketOptionLevel.IP,
                SocketOptionName.AddMembership,
                mcastOptionIpv4);

            _multicastMemberships.Add(mcastAddress, true);
        }

        public void MulticastDropMembership(string ipLan, string mcastAddress)
        {
            if (!_isMulticastInitialized) throw new ArgumentException("Multicast interface must be initialized before dropping multicast memberships");

            if (!_multicastMemberships.ContainsKey(mcastAddress)) return;

            if (!_multicastMemberships[mcastAddress].Equals(true)) return;

            var mcastOptionIpv4 = new MulticastOption(IPAddress.Parse(mcastAddress), IPAddress.Parse(ipLan));

            BackingUdpClient.Client.SetSocketOption(
                SocketOptionLevel.IP,
                SocketOptionName.DropMembership,
                mcastOptionIpv4);

            _multicastMemberships[mcastAddress] = false;
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
