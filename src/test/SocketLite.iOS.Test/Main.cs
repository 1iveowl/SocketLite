using SocketLite.Services;
using UIKit;

namespace SocketLite.iOS.Test
{
    public class Application
    {
        // This is the main entry point of the application.
        static void Main(string[] args)
        {

            var m_objTCP_Client = new TcpSocketClient();
            m_objTCP_Client.ConnectAsync("192.168.0.12", "80").Wait();

            var t = "";
            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.
            UIApplication.Main(args, null, "AppDelegate");


        }
    }
}