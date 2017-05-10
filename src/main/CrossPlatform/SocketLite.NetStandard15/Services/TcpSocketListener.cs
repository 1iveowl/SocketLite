﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using ISocketLite.PCL.Interface;
using SocketLite.Services.Base;
using SocketLite.Model;

#if !(NETSTANDARD1_5) //Not NetStandard
using CommunicationInterface = SocketLite.Model.CommunicationsInterface;
#endif

using PlatformSocketException = System.Net.Sockets.SocketException;
using PclSocketException = ISocketLite.PCL.Exceptions.SocketException;

namespace SocketLite.Services
{
    public class TcpSocketListener : TcpSocketBase, ITcpSocketListener
    {
        #region Obsolete

        private readonly ISubject<ITcpSocketClient> _subjectTcpSocket = new Subject<ITcpSocketClient>();

        [Obsolete("Deprecated, please use CreateObservableListener instead")]
        public IObservable<ITcpSocketClient> ObservableTcpSocket => _subjectTcpSocket.AsObservable();

        [Obsolete("Deprecated, please use CreateObservableListener instead")]
#pragma warning disable 1998
        public async Task StartListeningAsync(
#pragma warning restore 1998

            int port,
            ICommunicationInterface listenOn = null,
            bool allowMultipleBindToSamePort = false)
        {
            CheckCommunicationInterface(listenOn);

            var ipAddress = (listenOn as CommunicationsInterface)?.NativeIpAddress ?? IPAddress.Any;

            //_tcpListener = new TcpListener(ipAddress, port);

            try
            {
                _tcpListener = new TcpListener(ipAddress, port)
                {
                    ExclusiveAddressUse = !allowMultipleBindToSamePort
                };

            }
            catch (SocketException)
            {
                _tcpListener = new TcpListener(ipAddress, port);
                // Not all platforms need or accept the ExclusiveAddressUse option. Here we catch the exception if the platform does not need it.
            }

            try
            {
                _tcpListener.Start();
            }
            catch (PlatformSocketException ex)
            {
                throw new PclSocketException(ex);
            }

            _tcpClientSubscribe = ObservableTcpSocketFromAsync.Subscribe(
                client =>
                {
                    _subjectTcpSocket.OnNext(client);
                },
                ex =>
                {
                    Cleanup();
                    _subjectTcpSocket.OnError(ex);
                },
                () =>
                {
                    Cleanup();
                });
        }

        [Obsolete("Deprecated, please use CreateObservableListener instead")]
        public void StopListening()
        {
            Cleanup();
        }

        public void Dispose()
        {
            _tcpClientSubscribe?.Dispose();
            _tcpListener?.Stop();
            _listenCanceller?.Cancel();
        }

        private void Cleanup()
        {
            _listenCanceller.Cancel();
            try
            {
                _tcpListener?.Stop();
            }
            catch (PlatformSocketException ex)
            {
                throw new PclSocketException(ex);
            }

            _tcpListener = null;
        }

        #endregion

        private IObservable<ITcpSocketClient> ObservableTcpSocketFromAsync => ObserveTcpClientFromAsync.Select(
            tcpClient =>
            {
                var client = new TcpSocketClient(tcpClient, BufferSize);
                return client;
            })
            .Where(tcpClient => tcpClient != null);

        private IObservable<TcpClient> ObserveTcpClientFromAsync => Observable.While(
                () => !_listenCanceller.IsCancellationRequested,
                Observable.FromAsync(GetTcpClientAsync));

        private TcpListener _tcpListener;
        private readonly CancellationTokenSource _listenCanceller = new CancellationTokenSource();
        private IDisposable _tcpClientSubscribe;

        public int LocalPort => ((IPEndPoint)_tcpListener.LocalEndpoint).Port;

        public TcpSocketListener() : base(0)
        {
        }

        private async Task<TcpClient> GetTcpClientAsync()
        {
            TcpClient tcpClient = null;
            try
            {
                tcpClient = await _tcpListener.AcceptTcpClientAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
            return tcpClient;
        }

        public async Task<IObservable<ITcpSocketClient>> CreateObservableListener(
            int port,
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = false)
        {
            CheckCommunicationInterface(communicationInterface);

            var ipAddress = (communicationInterface as CommunicationsInterface)?.NativeIpAddress ?? IPAddress.Any;

            //_tcpListener = new TcpListener(ipAddress, port);

            try
            {
                _tcpListener = new TcpListener(ipAddress, port)
                {
                    ExclusiveAddressUse = !allowMultipleBindToSamePort
                };

            }
            catch (SocketException)
            {
                _tcpListener = new TcpListener(ipAddress, port);
                // Not all platforms need or accept the ExclusiveAddressUse option. Here we catch the exception if the platform does not need it.
            }

            try
            {
                _tcpListener.Start();
            }
            catch (PlatformSocketException ex)
            {
                throw new PclSocketException(ex);
            }

            var observable = Observable.Create<ITcpSocketClient>(
                obs =>
                {
                    var disp = Observable.While(
                        () => !_listenCanceller.IsCancellationRequested,
                        Observable.FromAsync(GetTcpClientAsync))
                        .Where(tcpClient => tcpClient != null)
                        .Subscribe(
                            tcpClient =>
                            {
                                var client = new TcpSocketClient(tcpClient, BufferSize);
                                obs.OnNext(client);
                            },
                            ex =>
                            {
                                Cleanup();
                                _listenCanceller.Cancel();
                                obs.OnError(ex);
                            },
                            () =>
                            {
                                _listenCanceller.Cancel();
                                Cleanup();
                            });

                    return disp;
                });

            return observable;
        }
    }
}
