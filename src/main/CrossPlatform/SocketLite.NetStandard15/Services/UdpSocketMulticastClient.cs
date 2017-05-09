﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ISocketLite.PCL.EventArgs;
using ISocketLite.PCL.Interface;
using SocketLite.Services.Base;
//#if (NETSTANDARD1_5) //Not NetStandard
//using CommunicationInterface = SocketLite.Model.CommunicationsInterface;
//#endif
using PlatformSocketException = System.Net.Sockets.SocketException;
using PclSocketException = ISocketLite.PCL.Exceptions.SocketException;

namespace SocketLite.Services
{
    public class UdpSocketMulticastClient : UdpSocketBase, IUdpSocketMulticastClient
    {

        #region Obsolete

        [Obsolete("Deprecated, please use CreateObservableMulticastListener instead")]
        public async Task JoinMulticastGroupAsync(
            string multicastAddress,
            int port,
            ICommunicationInterface communicationsInterface = null,
            bool allowMultipleBindToSamePort = false)
        {
            CheckCommunicationInterface(communicationsInterface);

            _ipEndPoint = UdpClientInitialize(
                communicationsInterface,
                port,
                allowMultipleBindToSamePort,
                isUdpMultiCast: true,
                mcastAddress: multicastAddress);

            _cancellationTokenSource = new CancellationTokenSource();

            _multicastAddress = multicastAddress;
            _multicastPort = port;

            await Task.Run(() => RunMessageReceiver(_cancellationTokenSource.Token)).ConfigureAwait(false);
        }

        [Obsolete("Deprecated, please use CreateObservableMulticastListener instead")]
        public void Disconnect()
        {
            Cleanup();
        }

        #endregion

        private IPEndPoint _ipEndPoint;

        public UdpSocketMulticastClient()
        {
        }

        private string _multicastAddress;
        private int _multicastPort;
        private CancellationTokenSource _cancellationTokenSource;

        public int TTL { get; set; } = 1;

        public int Port => _ipEndPoint.Port;
        public string IpAddress => _ipEndPoint.Address.ToString();


        public async Task<IObservable<IUdpMessage>> ObservableMulticastListener(
            string multicastAddress,
            int port,
            ICommunicationInterface communicationsInterface = null,
            bool allowMultipleBindToSamePort = false)
        {
            CheckCommunicationInterface(communicationsInterface);

            _ipEndPoint = UdpClientInitialize(
                communicationsInterface,
                port,
                allowMultipleBindToSamePort,
                isUdpMultiCast: true,
                mcastAddress: multicastAddress);

            _cancellationTokenSource = new CancellationTokenSource();

            _multicastAddress = multicastAddress;
            _multicastPort = port;

            return CreateObservableMessageStream(_cancellationTokenSource);
        }

        protected override void Cleanup()
        {
            _cancellationTokenSource.Cancel();

#if (NETSTANDARD1_5)
            BackingUdpClient?.Dispose();
#else
            BackingUdpClient?.Close();
#endif

            _multicastAddress = null;
            _multicastPort = 0;
            base.Cleanup();
        }

        public async Task SendMulticastAsync(byte[] data)
        {
            await SendMulticastAsync(data, data.Length).ConfigureAwait(false);
        }

        public async Task SendMulticastAsync(byte[] data, int length)
        {
            if (_multicastAddress == null)
                throw new InvalidOperationException("Must join a multicast group before sending.");

            await base.SendToAsync(data, length, _multicastAddress, _multicastPort).ConfigureAwait(false);
        }
    }
}
