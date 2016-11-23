using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.Marshal;

namespace SocketLite.Helper
{
    // http://www.pinvoke.net/default.aspx/iphlpapi/GetAdaptersInfo.html
    internal static class AdapterInfo
    {

        const int MAX_ADAPTER_DESCRIPTION_LENGTH = 128;
        const int ERROR_BUFFER_OVERFLOW = 111;
        const int MAX_ADAPTER_NAME_LENGTH = 256;
        const int MAX_ADAPTER_ADDRESS_LENGTH = 8;
        const int MIB_IF_TYPE_OTHER = 1;
        const int MIB_IF_TYPE_ETHERNET = 6;
        const int MIB_IF_TYPE_TOKENRING = 9;
        const int MIB_IF_TYPE_FDDI = 15;
        const int MIB_IF_TYPE_PPP = 23;
        const int MIB_IF_TYPE_LOOPBACK = 24;
        const int MIB_IF_TYPE_SLIP = 28;


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct IP_ADAPTER_INFO
        {
            public IntPtr Next;
            public int ComboIndex;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ADAPTER_NAME_LENGTH + 4)]
            public string AdapterName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ADAPTER_DESCRIPTION_LENGTH + 4)]
            public string AdapterDescription;
            public uint AddressLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_ADAPTER_ADDRESS_LENGTH)]
            public byte[] Address;
            public int Index;
            public uint Type;
            public uint DhcpEnabled;
            public IntPtr CurrentIpAddress;
            public IP_ADDR_STRING IpAddressList;
            public IP_ADDR_STRING GatewayList;
            public IP_ADDR_STRING DhcpServer;
            public bool HaveWins;
            public IP_ADDR_STRING PrimaryWinsServer;
            public IP_ADDR_STRING SecondaryWinsServer;
            public int LeaseObtained;
            public int LeaseExpires;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct IP_ADDR_STRING
        {
            public IntPtr Next;
            public IP_ADDRESS_STRING IpAddress;
            public IP_ADDRESS_STRING IpMask;
            public int Context;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct IP_ADDRESS_STRING
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string Address;
        }

        [DllImport("iphlpapi.dll", CharSet = CharSet.Ansi)]
        public static extern int GetAdaptersInfo(IntPtr pAdapterInfo, ref long pBufOutLen);
        public static IEnumerable<AdvancedAdapterInfo> GetAdapters()
        {
            var advancedAdapterList = new List<AdvancedAdapterInfo>();

            long structSize = SizeOf<IP_ADAPTER_INFO>();

            var pArray = AllocHGlobal(new IntPtr(structSize));

            var ret = GetAdaptersInfo(pArray, ref structSize);

            if (ret == ERROR_BUFFER_OVERFLOW) // ERROR_BUFFER_OVERFLOW == 111
            {
                // Buffer was too small, reallocate the correct size for the buffer.
                pArray = ReAllocHGlobal(pArray, new IntPtr(structSize));

                ret = GetAdaptersInfo(pArray, ref structSize);
            } // if

            if (ret == 0)
            {
                // Call Succeeded
                var pEntry = pArray;

                do
                {
                    // Retrieve the adapter info from the memory address
                    var entry = PtrToStructure<IP_ADAPTER_INFO>(pEntry);

                    // ***Do something with the data HERE!***
                    //Console.WriteLine("\n");
                    //Console.WriteLine("Index: {0}", entry.Index.ToString());

                    // Adapter Type
                    var networkType = string.Empty;
                    switch (entry.Type)
                    {
                        case MIB_IF_TYPE_ETHERNET: networkType = "Ethernet"; break;
                        case MIB_IF_TYPE_TOKENRING: networkType = "Token Ring"; break;
                        case MIB_IF_TYPE_FDDI: networkType = "FDDI"; break;
                        case MIB_IF_TYPE_PPP: networkType = "PPP"; break;
                        case MIB_IF_TYPE_LOOPBACK: networkType = "Loopback"; break;
                        case MIB_IF_TYPE_SLIP: networkType = "Slip"; break;
                        default: networkType = "Other/Unknown"; break;
                    } // switch
                    //Console.WriteLine("Adapter Type: {0}", tmpString);

                    //Console.WriteLine("Name: {0}", entry.AdapterName);
                    //Console.WriteLine("Desc: {0}\n", entry.AdapterDescription);

                    //Console.WriteLine("DHCP Enabled: {0}", (entry.DhcpEnabled == 1) ? "Yes" : "No");

                    if (entry.DhcpEnabled == 1)
                    {
                        //Console.WriteLine("DHCP Server : {0}", entry.DhcpServer.IpAddress.Address);

                        // Lease Obtained (convert from "time_t" to C# DateTime)
                        var pdatDate = new DateTime(1970, 1, 1).AddSeconds(entry.LeaseObtained).ToLocalTime();
                        //Console.WriteLine("Lease Obtained: {0}", pdatDate.ToString());

                        // Lease Expires (convert from "time_t" to C# DateTime)
                        pdatDate = new DateTime(1970, 1, 1).AddSeconds(entry.LeaseExpires).ToLocalTime();
                        //Console.WriteLine("Lease Expires : {0}\n", pdatDate.ToString());
                    } // if DhcpEnabled

                    //Console.WriteLine("IP Address     : {0}", entry.IpAddressList.IpAddress.Address);
                    //Console.WriteLine("Subnet Mask    : {0}", entry.IpAddressList.IpMask.Address);
                    //Console.WriteLine("Default Gateway: {0}", entry.GatewayList.IpAddress.Address);

                    // MAC Address (data is in a byte[])
                    networkType = string.Empty;
                    for (var i = 0; i < entry.AddressLength - 1; i++)
                    {
                        networkType += $"{entry.Address[i]:X2}-";
                    }
                    //Console.WriteLine("MAC Address    : {0}{1:X2}\n", tmpString, entry.Address[entry.AddressLength - 1]);

                    //Console.WriteLine("Has WINS: {0}", entry.HaveWins ? "Yes" : "No");
                    if (entry.HaveWins)
                    {
                        //Console.WriteLine("Primary WINS Server  : {0}", entry.PrimaryWinsServer.IpAddress.Address);
                        //Console.WriteLine("Secondary WINS Server: {0}", entry.SecondaryWinsServer.IpAddress.Address);
                    } // HaveWins

                    // Get next adapter (if any)
                    pEntry = entry.Next;
                    string gateway = null;

                    if (entry.GatewayList.IpAddress.Address != "0.0.0.0" &&
                        !string.IsNullOrEmpty(entry.GatewayList.IpAddress.Address))
                    {
                        gateway = entry.GatewayList.IpAddress.Address;
                    }
                    

                    var adpterInfo = new AdvancedAdapterInfo
                    {
                        AdapterDescription = entry.AdapterDescription,
                        AdapterId = entry.AdapterName.ToLower().Replace("{", string.Empty).Replace("}", string.Empty),
                        Gateway = gateway,
                        HasDhcp = entry.DhcpEnabled == 1
                    };
                    advancedAdapterList.Add(adpterInfo);

                }
                while (pEntry != IntPtr.Zero);

                FreeHGlobal(pArray);

            } // if
            else
            {
                FreeHGlobal(pArray);
                throw new InvalidOperationException("GetAdaptersInfo failed: " + ret);
            }
            return advancedAdapterList;
        }
    }
}
