﻿using System;
using System.IO;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking;
using Windows.Networking.Sockets;
using ISocketLite.PCL.EventArgs;
using ISocketLite.PCL.Interface;
using SocketLite.Model;

namespace SocketLite.Services.Base
{
    public abstract class UdpSocketBase : UdpSendBase
    {
        // Using a subject to keep ensure that a connection can be closed and reopened while keeping subscribing part intact
        private readonly ISubject<IUdpMessage> _messageSubjekt = new Subject<IUdpMessage>();

        private IDisposable _messageSubscribe;

        public IObservable<IUdpMessage> ObservableMessages => _messageSubjekt.AsObservable();

        private IObservable<IUdpMessage> ObserveMessagesFromEvents
            => Observable.FromEventPattern<
                TypedEventHandler<DatagramSocket, DatagramSocketMessageReceivedEventArgs>,
                DatagramSocketMessageReceivedEventArgs>(
                ev => DatagramSocket.MessageReceived += ev,
                ev => DatagramSocket.MessageReceived -= ev)
                .Select(
                    handler =>
                    {
                        var remoteAddress = handler.EventArgs.RemoteAddress.CanonicalName;
                        var remotePort = handler.EventArgs.RemotePort;
                        byte[] allBytes;

                        var stream = handler.EventArgs.GetDataStream().AsStreamForRead();
                        using (var memoryStream = new MemoryStream())
                        {
                            stream.CopyTo(memoryStream);
                            allBytes = memoryStream.ToArray();
                        }

                        return new UdpMessage
                        {
                            ByteData = allBytes,
                            RemoteAddress = remoteAddress,
                            RemotePort = remotePort
                        };
                    });

        protected UdpSocketBase()
        {
            InitializeUdpSocket();
            SubsribeToMessages();

        }

        protected async Task BindeUdpServiceNameAsync(
            ICommunicationInterface communicationInterface,
            string serviceName,
            bool allowMultipleBindToSamePort)
        {
           

            ConfigureDatagramSocket(allowMultipleBindToSamePort);

            var adapter = (communicationInterface as CommunicationsInterface)?.NativeNetworkAdapter;

            if (adapter != null)
            {
                await DatagramSocket.BindServiceNameAsync(serviceName, adapter);
            }
            else
            {
                await DatagramSocket.BindServiceNameAsync(serviceName);
            }
        }



        protected void CloseSocket()
        {
            DatagramSocket.Dispose();
            _messageSubscribe.Dispose();
            InitializeUdpSocket();
            SubsribeToMessages();
        }

        //protected void InitializeUdpSocket()
        //{
        //    DatagramSocket = new DatagramSocket();
        //    //SubsribeToMessages();
        //}

        private void SubsribeToMessages()
        {
            _messageSubscribe = ObserveMessagesFromEvents.Subscribe(
                msg =>
                {
                    _messageSubjekt.OnNext(msg);
                },
                ex =>
                {
                    _messageSubjekt.OnError(ex);
                },
                Dispose);
        }

        public void Dispose()
        {
            _messageSubscribe?.Dispose();
            DatagramSocket?.Dispose();
        }
    }
}
