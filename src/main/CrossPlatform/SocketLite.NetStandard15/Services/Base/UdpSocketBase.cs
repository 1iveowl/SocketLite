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
        }

        private IPEndPoint InitializeMulticast(IPEndPoint ipEndPoint, IPAddress ipLanIpAddress, string mcastAddress, bool allowMultipleBindToSamePort)
        {
            BackingUdpClient = new UdpClient(ipEndPoint)
            {
                EnableBroadcast = true,
                //ExclusiveAddressUse = false,
               };

            //if (allowMultipleBindToSamePort) SetAllowMultipleBindToSamePort(ipLanIpAddress);

            _isMulticastInitialized = true;

            MulticastAddMembership(ipEndPoint.Address.ToString(), mcastAddress);

            return ipEndPoint;
        }

        private IPEndPoint InitializeUnicase(IPEndPoint ipEndPoint, IPAddress ipLanIpAddress, bool allowMultipleBindToSamePort)
        {

            _isUnicastInitialized = true;
            BackingUdpClient = new UdpClient();

            if (allowMultipleBindToSamePort) SetAllowMultipleBindToSamePort(ipLanIpAddress);

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

             BackingUdpClient.JoinMulticastGroup(IPAddress.Parse(mcastAddress));

            _multicastMemberships.Add(mcastAddress, true);
        }

        public void MulticastDropMembership(string ipLan, string mcastAddress)
        {
            if (!_isMulticastInitialized) throw new ArgumentException("Multicast interface must be initialized before dropping multicast memberships");

            if (!_multicastMemberships.ContainsKey(mcastAddress)) return;

            if (!_multicastMemberships[mcastAddress].Equals(true)) return;

            BackingUdpClient.DropMulticastGroup(IPAddress.Parse(mcastAddress));

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
