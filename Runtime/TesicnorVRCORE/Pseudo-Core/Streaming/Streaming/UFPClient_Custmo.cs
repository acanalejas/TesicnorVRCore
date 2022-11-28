using System;
using System.Net.Sockets;
using System.Net;

public class UFPClient_Custmo
{
    public static void SendData(byte[] data, string IP)
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Tcp);
        IPAddress broadcast = IPAddress.Parse(IP);

        IPEndPoint remoteEP = new IPEndPoint(broadcast, 8080);
        socket.SendTo(data, remoteEP);
    }
}
