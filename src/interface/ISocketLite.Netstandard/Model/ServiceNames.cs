﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISocketLite.PCL.Model
{
    public static class ServiceNames
    {
        public static ushort PortForTcpServiceName(string sn)
        {
            ushort port = 0;

            if (ushort.TryParse(sn, out port))
            {
                return port;
            }

            if (!TcpServiceNames.TryGetValue(sn, out port))
            {
                return port;
            }
            else
            {
                throw new ArgumentException($"Invalid service name or port number: {sn}");
            }
            
        }

        private static readonly Dictionary<string, ushort> TcpServiceNames = new Dictionary<string, ushort>
        {
            {"echo", 7},
            {"discard", 9},
            {"systat", 11},
            {"daytime", 13},
            {"qotd", 17},
            {"chargen", 19},
            {"ftp-data", 20},
            {"ftp", 21},
            {"ssh", 22},
            {"telnet", 23},
            {"smtp", 25},
            {"time", 37},
            {"nameserver", 42},
            {"nicname", 43},
            {"domain", 53},
            {"gopher", 70},
            {"finger", 79},
            {"http", 80},
            {"hosts2-ns", 81},
            {"kerberos", 88},
            {"hostname", 101},
            {"iso-tsap", 102},
            {"rtelnet", 107},
            {"pop2", 109},
            {"pop3", 110},
            {"sunrpc", 111},
            {"auth", 113},
            {"uucp-path", 117},
            {"sqlserv", 118},
            {"nntp", 119},
            {"epmap", 135},
            {"netbios-ns", 137},
            {"netbios-ssn", 139},
            {"imap", 143},
            {"sql-net", 150},
            {"sqlsrv", 156},
            {"pcmail-srv", 158},
            {"print-srv", 170},
            {"bgp", 179},
            {"irc", 194},
            {"rtsps", 322},
            {"mftp", 349},
            {"ldap", 389},
            {"https", 443},
            {"microsoft-ds", 445},
            {"kpasswd", 464},
            {"crs", 507},
            {"exec", 512},
            {"login", 513},
            {"cmd", 514},
            {"printer", 515},
            {"efs", 520},
            {"ulp", 522},
            {"tempo", 526},
            {"irc-serv", 529},
            {"courier", 530},
            {"conference", 531},
            {"netnews", 532},
            {"uucp", 540},
            {"klogin", 543},
            {"kshell", 544},
            {"dhcpv6-client", 546},
            {"dhcpv6-server", 547},
            {"afpovertcp", 548},
            {"rtsp", 554},
            {"remotefs", 556},
            {"nntps", 563},
            {"whoami", 565},
            {"ms-shuttle", 568},
            {"ms-rome", 569},
            {"http-rpc-epmap", 593},
            {"hmmp-ind", 612},
            {"hmmp-op", 613},
            {"ldaps", 636},
            {"doom", 666},
            {"msexch-routing", 691},
            {"kerberos-adm", 749},
            {"mdbs_daemon", 800},
            {"ftps-data", 989},
            {"ftps", 990},
            {"telnets", 992},
            {"imaps", 993},
            {"ircs", 994},
            {"pop3s", 995},
            {"kpop", 1109},
            {"nfsd-status", 1110},
            {"nfa", 1155},
            {"activesync", 1034},
            {"opsmgr", 1270},
            {"ms-sql-s", 1433},
            {"ms-sql-m", 1434},
            {"ms-sna-server", 1477},
            {"ms-sna-base", 1478},
            {"wins", 1512},
            {"ingreslock", 1524},
            {"stt", 1607},
            {"pptconference", 1711},
            {"pptp", 1723},
            {"msiccp", 1731},
            {"remote-winsock", 1745},
            {"ms-streaming", 1755},
            {"msmq", 1801},
            {"msnp", 1863},
            {"ssdp", 1900},
            {"close-combat", 1944},
            {"knetd", 2053},
            {"mzap", 2106},
            {"qwave", 2177},
            {"directplay", 2234},
            {"ms-olap3", 2382},
            {"ms-olap4", 2383},
            {"ms-olap1", 2393},
            {"ms-olap2", 2394},
            {"ms-theater", 2460},
            {"wlbs", 2504},
            {"ms-v-worlds", 2525},
            {"sms-rcinfo", 2701},
            {"sms-xfer", 2702},
            {"sms-chat", 2703},
            {"sms-remctrl", 2704},
            {"msolap-ptp2", 2725},
            {"icslap", 2869},
            {"cifs", 3020},
            {"xbox", 3074},
            {"ms-dotnetster", 3126},
            {"ms-rule-engine", 3132},
            {"msft-gc", 3268},
            {"msft-gc-ssl", 3269},
            {"ms-cluster-net", 3343},
            {"ms-wbt-server", 3389},
            {"ms-la", 3535},
            {"pnrp-port", 3540},
            {"teredo", 3544},
            {"p2pgroup", 3587},
            {"ws-discovery", 3702},
            {"dvcprov-port", 3776},
            {"msfw-control", 3847},
            {"msdts1", 3882},
            {"sdp-portmapper", 3935},

            {"net-device", 4350},
            {"ipsec-msft", 4500},
            {"llmnr", 5355},
            {"wsd", 5357},

            {"rrac", 5678},
            {"dccm", 5679},

            {"ms-licensing", 5720},
            {"directplay8", 6073},
            {"man", 9535},
            {"rasadv", 9753},
            {"imip-channels", 11320},
            {"directplaysrvr", 47624},
        };
    }
}
