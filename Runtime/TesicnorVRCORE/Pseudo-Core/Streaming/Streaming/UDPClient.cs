using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Policy;
using System.Threading.Tasks;
using UnityEngine;



namespace StreamingCSharp
{
    public class UDPClient
    {
        private static UdpClient client;
        public static IPEndPoint remoteEndPoint;

        public static string ip = "192.168.1.13";
        private static int port = 8080;
        private static byte[] content;
        public static bool isStreaming;

        public static void InitializeClient()
        {
            try
            {
                CloseSocket();
                client = new UdpClient();
                client.EnableBroadcast = true;

                remoteEndPoint = new IPEndPoint(IPAddress.Parse("255.255.255.255"), port);

                client.Client.SendBufferSize = 1024 * 1024;

                Application.quitting += () => { CloseSocket(); };
            }
            catch (Exception e)
            {
                Debug.LogError("No se ha podido inicializar el cliente UDP debido a : " + e.Message);
            }
            
        }

        public static async Task SendBytes(byte[] _b)
        {
            if (client == null) return;

            await client.SendAsync(_b, _b.Length, remoteEndPoint);
        }

        public static void CloseSocket()
        {
            if (client != null)
            {
                client.Close();
            }
        }
    }
}