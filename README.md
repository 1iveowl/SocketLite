# SocketLite - .NET Standard

[![NuGet Badge](https://buildstats.info/nuget/SocketLite.PCL)](https://www.nuget.org/packages/SocketLite.PCL)

[![.NET Standard](http://img.shields.io/badge/.NET_Standard-v2.0-red.svg)](https://docs.microsoft.com/da-dk/dotnet/articles/standard/library) 


*Please star this project if you find it useful. Thank you.*

### No longer updated or supported
With the level of maturity .NET Standard have reached, I don't see the real need for this library anymore. If you think you need this library please consider if maybe there's a smarter way to accomplish what you need to accomplish.

### Supports Xamarin Forms on Windows 10/UWP (16299 and later), iOS, Android, and .NET Standard from 2.0

This project is a fork that build upon the fantastic work done with [Socket for PCL](https://github.com/rdavisau/sockets-for-pcl). 

## Why this fork? 
Two reasons:

 1. The original Socket for PCL delivers great broad cross-platform
    support. SocketLite PCL only covers .NET 4.5+, UWP, iOS and Android.
    
 2. SocketLite has been refactored to use Reactive Extensions (Rx).

The purpose of this PCLis to make it easy to write cross platform socket code. For example [Simple Http Listener](https://github.com/1iveowl/Simple-Http-Listener-PCL) is written using SocketLite.PCL

This library is based on "Bait and Switch" pattern. It is strongly recommend to read this short and great blog post to get an good understanding of this pattern before contributing to the SocketLite PCL code-base: [The Bait and Switch PCL Trick](http://log.paulbetts.org/the-bait-and-switch-pcl-trick/)

Get SocketLite.PCL in NuGet: ````Install-Package SocketLite.PCL````

### Version 5.0
Version 5.0 updated System.Reactive to version 4.0.0
.NET is support from version 4.6 and later.

.NET Standard 1.3 amd 1.5 is no longer part of the library, only .NET Standard 2.0 and later is supported.

### Version 4.0
Version 4.0 represents a major overhaul of this library. Version 4.0 is still backwards compatible, but many of the methods have been marked as deprecated to inspire developers to use the newer versions of this library. In previous versions you had to subscribe to an observable and then start the action. In version 4.0 you just subscribe, that's it. Much more clean and better aligned with the Rx patterns.

There is still UWP support in version 4.0, but the emphasis has been on .NET Core and it will be going forward.

### Classes
The plugin currently provides the following socket abstractions:

Class|Description|.NET|Windows 10 / UWP
-----|-----------|:--------------:|:---------------:
**TcpSocketListener** | Bind to a port and accept TCP socket connections. | TcpListener | StreamSocketListener 
**TcpSocketClient** | Connect to a TCP endpoint with bi-directional communication. | TcpClient | StreamSocket
**UdpSocketReceiver** | Bind to a port and receive UDP messages. | UdpClient | DatagramSocket
**UdpSocketClient** | Send messages to arbitrary endpoints over UDP. | UdpClient | DatagramSocket
**UdpSocketMulticastClient** | Send and receive UDP messages within a multicast group. | UdpClient | DatagramSocket

### Examples Usage

#### Using

```csharp
using SocketLite.Model;
using SocketLite.Services;
```

**IMPORTANT**: Please notice that the parameter `allowMultipleBindToSamePort` will only work on Windows. On other platforms it should be set to `false`.

#### Specify Which Network Interface To Use
Defining what Network Interface to use is typically the version first step. This can be done on multiple ways. Here it is done using the IP adress of the interface:

```csharp
var communicationInterface = new CommunicationsInterface();
var allInterfaces = communicationInterface.GetAllInterfaces();
var networkInterface = allInterfaces.FirstOrDefault(x => x.IpAddress == "192.168.0.2");
```

##### A TCP Listener
```csharp
var tcpListener = new TcpSocketListener();

var observerTcpListner = await tcpListener.CreateObservableListener(
    port:8000, 
    listenOn: networkInterface, 
    allowMultipleBindToSamePort:false);

var subscriberTcpListener = observerTcpListner.Subscribe(
    tcpClient =>
    {
        //Insert your code here
    },
    ex =>
    {
        // Insert your exception code here
    },
    () =>
    {
        // Insert your completed code here
    });
```


##### A TCP Client
```csharp
var tcpClient = new TcpSocketClient();
await tcpClient.ConnectAsync("192.168.1.100", "1234");

var helloWorld = "Hello World!";

var bytes = Encoding.UTF8.GetBytes(helloWorld);
await tcpClient.WriteStream.WriteAsync(bytes, 0, bytes.Length);
tcpClient.Disconnect();
```
###### Tip: What About Multi Threading?
The TcpSocketClient WriteStream property is of type ```System.IO.Stream```, which does not automatically manage multi-treading. 

To work around this limitation you can do something like the following example, which have been inspired by this Stack Overflow post: [SslStream.WriteAsync “The BeginWrite method cannot be called when another write operation is pending”](http://stackoverflow.com/a/12649107/4140832)

```csharp

private readonly ConcurrentQueue<byte[]> _writePendingData = new ConcurrentQueue<byte[]>();
private bool _sendingData;

private async Task SendAsync(ITcpSocketClient tcpSocketClient, byte[] frame)
{
    if (!tcpSocketClient.IsConnected)
    {
        throw new Exception("Websocket connection have been closed");
    }

    await WriteQueuedStreamAsync(tcpSocketClient, frame);
}       

private async Task WriteQueuedStreamAsync(ITcpSocketClient tcpSocketClient, byte[] frame)
{
    if (frame == null)
    {
        return;
    }

    _writePendingData.Enqueue(frame);

    lock (_writePendingData)
    {
        if (_sendingData)
        {
            return;
        }
        _sendingData = true;
    }

    try
    {
        if (_writePendingData.Count > 0 && _writePendingData.TryDequeue(out byte[] buffer))
        {
            await tcpSocketClient.WriteStream.WriteAsync(buffer, 0, buffer.Length);
            await tcpSocketClient.WriteStream.FlushAsync();
        }
        else
        {
            lock (_writePendingData)
            {
                _sendingData = false;
            }
        }
    }
    catch (Exception)
    {
        // handle exception then
        lock (_writePendingData)
        {
            _sendingData = false;
        }
    }
    finally
    {
        lock (_writePendingData)
        {
            _sendingData = false;
        }
    }
}
```
    
##### A UDP Receiver
```csharp
var udpReceiver = new UdpSocketReceiver();

var observerUdpReceiver = await udpReceiver.ObservableUnicastListener(
    port: 1900,
    communicationInterface: networkInterface,
    allowMultipleBindToSamePort: false);

var subscriberUpdReceiver = observerUdpReceiver.Subscribe(
    udpMsg =>
    {
        System.Console.WriteLine($"Udp package received: {udpMsg.RemoteAddress}:{udpMsg.RemotePort}");
    },
    ex =>
    {
        //Inset your exception code here
    },
    () =>
    {
        //Insert your completion code here
    });
```

##### A UDP Client
```csharp
var udpReceiver = new UdpSocketReceiver();

var observerUdpReceiver = await udpReceiver.CreateObservableListener(
    port: 8000,
    communicationInterface: networkInterface,
    allowMultipleBindToSamePort: false);

var subscriberUpdReceiver = observerUdpReceiver.Subscribe(
    udpMsg =>
    {
        //Inset your code here
    },
    ex =>
    {
        //Inset your exception code here
    },
    () =>
    {
        //Insert your completion code here
    });

// Fire datagram into the great void
await udpClient.SendToAsync(bytes, bytes.Length, address:"192.168.1.5", port:1234);
```

##### A Multicast UDP Client
```csharp
var udpMulticast = new SocketLite.Services.UdpSocketMulticastClient();
await udpMulticast.JoinMulticastGroupAsync("239.255.255.250", 1900, allowMultipleBindToSamePort:true); //Listen for UPnP activity on local network.

// Listen part
var udpMulticast = new UdpSocketMulticastClient();

var observerUdpMulticast = await udpMulticast.ObservableMulticastListener(
    "239.255.255.250",
    1900,
    networkInterface,
    allowMultipleBindToSamePort: false);

var subscriberUdpMilticast = observerUdpMulticast.Subscribe(
    udpMsg =>
    {
        System.Console.WriteLine($"Udp package received: {udpMsg.RemoteAddress}:{udpMsg.RemotePort}");
    },
    ex =>
    {
        //Insert your exception code here
    },
    () =>
    {
        //Insert your completion code here
    });
```
You can also add or drop multicast addresses on the Multicast listeren using: `MulticastAddMembership(string ipLan, string mcastAddress)` and `MulticastDropMembership(string ipLan, string mcastAddress)`

Note: A UDP Multicast Client also listen to any other UDP send directly to the specified port. So if you need a UDP Client and a UDP Multicast Listener on the same port, all you need to do is to create one UDP Multicast Listener.

