using System;
using System.Reactive.Concurrency;
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
        private StreamSocketListener _streamSocketListener;
        
        private IDisposable _subscription;

        private readonly ISubject<ITcpSocketClient> _subjectTcpSocket = new Subject<ITcpSocketClient>();
        public IObservable<ITcpSocketClient> ObservableTcpSocket => _subjectTcpSocket.AsObservable();

        private IObservable<ITcpSocketClient> ObservableTcpSocketConnectionsFromEvents =>
            Observable.FromEventPattern<
                    TypedEventHandler<StreamSocketListener, StreamSocketListenerConnectionReceivedEventArgs>,
                    StreamSocketListenerConnectionReceivedEventArgs>(
                    ev => _streamSocketListener.ConnectionReceived += ev,
                    ev => _streamSocketListener.ConnectionReceived -= ev)
                .Select(handler => new TcpSocketClient(handler.EventArgs.Socket, BufferSize));
                //.ObserveOn(Scheduler.Default);
            //.Publish().RefCount();

        public int LocalPort { get; internal set; }

        public TcpSocketListener() : base(bufferSize:0)
        {
        }

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

        public void StopListening()
        {
           _subscription?.Dispose();
            _streamSocketListener?.Dispose();
        }

        public void Dispose()
        {
            StopListening();
        }
    }
}
