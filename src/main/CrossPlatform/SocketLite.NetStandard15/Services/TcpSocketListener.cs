using System;
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
        private readonly ISubject<ITcpSocketClient> _subjectTcpSocket = new Subject<ITcpSocketClient>();
        public IObservable<ITcpSocketClient> ObservableTcpSocket => _subjectTcpSocket.AsObservable();

        private IObservable<ITcpSocketClient> ObservableTcpSocketFromAsync => ObserveTcpClientFromAsync.Select(
            tcpClient =>
            {
                var client = new TcpSocketClient(tcpClient, BufferSize);
                return client;
            })
            .Where(tcpClient => tcpClient != null);

        private IObservable<TcpClient> ObserveTcpClientFromAsync => Observable.While(
            () => !_listenCanceller.IsCancellationRequested,
            Observable.FromAsync(GetTcpClientAsync))
            .ObserveOn(Scheduler.Default)
            .Publish().RefCount();

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
                    _subjectTcpSocket.OnError(ex);
                });
        }

        public void StopListening()
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

        public void Dispose()
        {
            _tcpClientSubscribe?.Dispose();
            _tcpListener?.Stop();
            _listenCanceller?.Cancel();
        }
    }
}
