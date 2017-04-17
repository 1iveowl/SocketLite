using System;
using System.Collections.Generic;
using System.Linq;
using ISocketLite.PCL.Interface;
using ISocketLite.PCL.Model;
using SocketLite.Model;

namespace SocketLite.Services.Base
{
    public abstract class CommonSocketBase
    {
        protected static readonly HashSet<Type> NativeSocketExceptions = new HashSet<Type> { typeof(System.Net.Sockets.SocketException) };
        protected void CheckCommunicationInterface(ICommunicationInterface communicationsInterface)
        {
            if (communicationsInterface != null && !communicationsInterface.IsUsable)
            {
                throw new InvalidOperationException("Cannot listen on an unusable communication interface.");
            }

            if (communicationsInterface == null) //Try and find best possible Network interface.
            {
                var newCommunicationsInterface = new CommunicationsInterface();
                var allInterfaces = newCommunicationsInterface.GetAllInterfaces();
                if (allInterfaces != null && allInterfaces.Any())
                {
                    var firstUsableCommunicationInterface = allInterfaces.FirstOrDefault(x => x.IsUsable && !x.IsLoopback && x.GatewayAddress != null);
                    if (firstUsableCommunicationInterface == null)
                    {
                        communicationsInterface = allInterfaces.FirstOrDefault(x => x.IsUsable);
                    }
                }
            }
        }
    }
}
