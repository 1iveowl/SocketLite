using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketLite.Helper
{
    internal class AdvancedAdapterInfo
    {
        internal string AdapterDescription { get; set; } 
        internal string AdapterId { get; set; }
        internal string Gateway { get; set; }
        internal bool HasDhcp { get; set; }
         
    }
}
