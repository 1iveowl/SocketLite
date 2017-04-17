using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISocketLite.PCL.Model
{
    public enum CommunicationConnectionStatus
    {

        Unknown,
        Connected,
        Disconnected,
        Testing,
        Dormant,
        NotPresent,
        LowerLayerDown,
    }
}

