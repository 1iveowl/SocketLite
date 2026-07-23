# SocketLite

[![NuGet](https://img.shields.io/nuget/v/SocketLite.PCL?logo=nuget&label=SocketLite.PCL)](https://www.nuget.org/packages/SocketLite.PCL)
[![Downloads](https://img.shields.io/nuget/dt/SocketLite.PCL?logo=nuget&color=blue)](https://www.nuget.org/packages/SocketLite.PCL)
[![License: MIT](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE.md)

[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.0-5C2D91?logo=dotnet&logoColor=white)](https://learn.microsoft.com/dotnet/standard/net-standard)

> **Warning**
> This library is no longer maintained. There are no plans to migrate it to newer platforms or extend it with new features. Consider whether the built-in .NET networking APIs meet your needs before adopting it.

A legacy cross-platform socket abstraction for Xamarin.Forms, UWP, iOS, Android, .NET Framework, and .NET Standard applications.

*Please star this project if you find it useful. Thank you.*

## Overview

SocketLite is a fork of [Sockets for PCL](https://github.com/rdavisau/sockets-for-pcl). It provides Rx-based socket abstractions for TCP, UDP, and multicast communication.

The library was created to simplify cross-platform socket code. For example, [Simple Http Listener](https://github.com/1iveowl/Simple-Http-Listener-PCL) uses SocketLite.PCL.

SocketLite uses the "Bait and Switch" PCL pattern. Before contributing to this legacy codebase, read [The Bait and Switch PCL Trick](http://log.paulbetts.org/the-bait-and-switch-pcl-trick/).

Install the package from NuGet:

```powershell
Install-Package SocketLite.PCL
```

## Release notes

### Version 5.0

- Updated System.Reactive to version 4.0.0.
- Supports .NET Framework 4.6 and later.
- Supports .NET Standard 2.0; .NET Standard 1.3 and 1.5 are no longer included.

### Version 4.0

Version 4.0 was a major but backwards-compatible overhaul. It simplified the Rx usage pattern: subscribing starts the action, rather than requiring a separate start call. UWP support remained, while the focus shifted to .NET Core and .NET Standard.

## Socket abstractions

The package provides the following socket abstractions:

Class|Description|.NET|Windows 10 / UWP
-----|-----------|:--------------:|:---------------:
**TcpSocketListener** | Bind to a port and accept TCP socket connections. | TcpListener | StreamSocketListener 
**TcpSocketClient** | Connect to a TCP endpoint with bi-directional communication. | TcpClient | StreamSocket
**UdpSocketReceiver** | Bind to a port and receive UDP messages. | UdpClient | DatagramSocket
**UdpSocketClient** | Send messages to arbitrary endpoints over UDP. | UdpClient | DatagramSocket
**UdpSocketMulticastClient** | Send and receive UDP messages within a multicast group. | UdpClient | DatagramSocket

## Examples

### Namespaces

```csharp
using SocketLite.Model;
using SocketLite.Services;
```

> **Important**
> `allowMultipleBindToSamePort` works only on Windows. Set it to `false` on other platforms.

### Select a network interface

Selecting a network interface is typically the first step. This example finds an interface by its IP address:

```csharp
var communicationInterface = new CommunicationsInterface();
var allInterfaces = communicationInterface.GetAllInterfaces();
var networkInterface = allInterfaces.FirstOrDefault(x => x.IpAddress == "192.168.0.2");
```

### TCP listener
```csharp
var tcpListener = new TcpSocketListener();

var observerTcpListner = await tcpListener.CreateObservableListener(
    port:8000, 
    listenOn: networkInterface, 
    allowMultipleBindToSamePort:false);

var subscriberTcpListener = observerTcpListner.Subscribe(
    tcpClient =>
    {
        // Handle the connected client.
    },
    ex =>
    {
        // Handle the exception.
    },
    () =>
    {
        // Handle completion.
    });
```


### TCP client
```csharp
var tcpClient = new TcpSocketClient();
await tcpClient.ConnectAsync("192.168.1.100", "1234");

var helloWorld = "Hello World!";

var bytes = Encoding.UTF8.GetBytes(helloWorld);
await tcpClient.WriteStream.WriteAsync(bytes, 0, bytes.Length);
tcpClient.Disconnect();
```
#### Tip: concurrent writes

`TcpSocketClient.WriteStream` is a `System.IO.Stream`, which does not coordinate concurrent writes.

Use a queue or another synchronization mechanism, such as the approach inspired by this Stack Overflow post: [SslStream.WriteAsync “The BeginWrite method cannot be called when another write operation is pending”](http://stackoverflow.com/a/12649107/4140832).

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
    
### UDP receiver
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
        // Handle the exception.
    },
    () =>
    {
        // Handle completion.
    });
```

### UDP client
```csharp
var udpReceiver = new UdpSocketReceiver();

var observerUdpReceiver = await udpReceiver.CreateObservableListener(
    port: 8000,
    communicationInterface: networkInterface,
    allowMultipleBindToSamePort: false);

var subscriberUpdReceiver = observerUdpReceiver.Subscribe(
    udpMsg =>
    {
        // Handle the message.
    },
    ex =>
    {
        // Handle the exception.
    },
    () =>
    {
        // Handle completion.
    });

// Send a datagram.
await udpClient.SendToAsync(bytes, bytes.Length, address:"192.168.1.5", port:1234);
```

### Multicast UDP client
```csharp
var udpMulticast = new SocketLite.Services.UdpSocketMulticastClient();
await udpMulticast.JoinMulticastGroupAsync("239.255.255.250", 1900, allowMultipleBindToSamePort:true); // Listen for local UPnP activity.

// Listen.
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
        // Handle the exception.
    },
    () =>
    {
        // Handle completion.
    });
```
You can add or remove multicast addresses with `MulticastAddMembership(string ipLan, string mcastAddress)` and `MulticastDropMembership(string ipLan, string mcastAddress)`.

> **Note**
> A UDP multicast client also receives UDP messages sent directly to its configured port. If you need both a UDP client and a multicast listener on the same port, create one multicast listener.
