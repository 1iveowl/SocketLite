using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking.Sockets;
using ISocketLite.PCL.Interface;
using SocketLite.Model;
using SocketLite.Services.Base;

namespace SocketLite.Services
{
    public class TcpSocketListener : TcpSocketBase, ITcpSocketListener
    {

        #region Obsolete

        private readonly ISubject<ITcpSocketClient> _subjectTcpSocket = new Subject<ITcpSocketClient>();

        [Obsolete("Deprecated, please use CreateObservableListener instead")]
        public IObservable<ITcpSocketClient> ObservableTcpSocket => _subjectTcpSocket.AsObservable();

        [Obsolete("Deprecated, please use CreateObservableListener instead")]
        public async Task StartListeningAsync(
            int port,
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = false)
        {
            //Throws and exception if the communication interface is not ready og valid.
            CheckCommunicationInterface(communicationInterface);

            _streamSocketListener = new StreamSocketListener();

            _subscription = ObservableTcpSocketConnectionsFromEvents.Subscribe(
                client =>
                {
                    _subjectTcpSocket.OnNext(client);
                },
                ex =>
                {
                    _subjectTcpSocket.OnError(ex);
                });

            var localServiceName = port == 0 ? "" : port.ToString();

            var adapter = (communicationInterface as CommunicationsInterface)?.NativeNetworkAdapter;

            if (adapter != null)
            {
                await _streamSocketListener.BindServiceNameAsync(
                    localServiceName, SocketProtectionLevel.PlainSocket,
                    adapter);
            }
            else
            {
                await _streamSocketListener.BindServiceNameAsync(localServiceName);
            }
        }

        [Obsolete("Deprecated, please use CreateObservableListener instead")]
        public void StopListening()
        {
            Cleanup();
        }

        #endregion

        private StreamSocketListener _streamSocketListener;
        
        private IDisposable _subscription;

        private IObservable<ITcpSocketClient> ObservableTcpSocketConnectionsFromEvents =>
            Observable.FromEventPattern<
                    TypedEventHandler<StreamSocketListener, StreamSocketListenerConnectionReceivedEventArgs>,
                    StreamSocketListenerConnectionReceivedEventArgs>(
                    ev => _streamSocketListener.ConnectionReceived += ev,
                    ev => _streamSocketListener.ConnectionReceived -= ev)
                .Select(handler => new TcpSocketClient(handler.EventArgs.Socket, BufferSize));

        public async Task<IObservable<ITcpSocketClient>> CreateObservableListener(
            int port, 
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = false)
        {
            CheckCommunicationInterface(communicationInterface);

            _streamSocketListener = new StreamSocketListener();


            var observable = Observable.Create<ITcpSocketClient>(
                obs =>
                {
                    var observeEvents = Observable.FromEventPattern<
                            TypedEventHandler<StreamSocketListener, StreamSocketListenerConnectionReceivedEventArgs>,
                            StreamSocketListenerConnectionReceivedEventArgs>(
                            ev => _streamSocketListener.ConnectionReceived += ev,
                            ev => _streamSocketListener.ConnectionReceived -= ev)
                        .Select(handler => new TcpSocketClient(handler.EventArgs.Socket, BufferSize));

                    var disp = observeEvents.Subscribe(
                        tcpClient =>
                        {
                            obs.OnNext(tcpClient);
                        },
                        ex =>
                        {
                            Cleanup();
                            obs.OnError(ex);
                        },
                        () =>
                        {
                            Cleanup();
                            obs.OnCompleted();
                        });
                    return disp;
                });

            var localServiceName = port == 0 ? "" : port.ToString();

            var adapter = (communicationInterface as CommunicationsInterface)?.NativeNetworkAdapter;

            if (adapter != null)
            {
                await _streamSocketListener.BindServiceNameAsync(
                    localServiceName, SocketProtectionLevel.PlainSocket,
                    adapter);
            }
            else
            {
                await _streamSocketListener.BindServiceNameAsync(localServiceName);
            }

            return observable;
        }

        public int LocalPort { get; internal set; }

        public TcpSocketListener() : base(bufferSize:0)
        {
        }

        public void Dispose()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            _subscription?.Dispose();
            _streamSocketListener?.Dispose();
        }
    }
}
