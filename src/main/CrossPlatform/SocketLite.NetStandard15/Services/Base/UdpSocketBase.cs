﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Concurrency;
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

        private readonly ISubject<IUdpMessage> _messageSubject = new Subject<IUdpMessage>();

        public IObservable<IUdpMessage> ObservableMessages => _messageSubject.AsObservable();

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
                }).SubscribeOn(Scheduler.Default);

            observeUdpReceive.Subscribe(
                // Message Received Args (OnNext)
                args =>
                {
                    _messageSubject.OnNext(args);
                },
                // Exception (OnError)
                ex =>
                {
                    throw (NativeSocketExceptions.Contains(ex.GetType()))
                        ? new SocketException(ex)
                        : ex;
                }, cancelToken);
        }

        protected IPEndPoint InitializeUdpClient(ICommunicationInterface communicationInterface, int port, bool allowMultipleBindToSamePort)
        {
            var ipAddress = (communicationInterface as CommunicationsInterface)?.NativeIpAddress ?? IPAddress.Any;

            var ipEndPoint = new IPEndPoint(ipAddress, port);

            BackingUdpClient = new UdpClient
            {
                EnableBroadcast = true,
            };

            var ipLan = IPAddress.Parse(ipEndPoint.Address.ToString());
            var bIp = ipLan.GetAddressBytes();

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
            MessageConcellationTokenSource?.Cancel();
#if (NETSTANDARD1_5)
            BackingUdpClient?.Dispose();
#else
            BackingUdpClient?.Close();
#endif
            _messageSubject?.OnCompleted();
        }
    }
}
