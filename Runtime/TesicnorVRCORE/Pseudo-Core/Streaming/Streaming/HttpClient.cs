using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO.Compression;
using System.IO;
using UnityEngine;


namespace StreamingCSharp
{
    public class HttpClient_Custom
    {
        public static string url = "http://192.168.20.55:8080";
        private static System.Net.Http.HttpClient client;
        private static byte[] content;
        public static bool isStreaming = false;

        public static void IntializeClient()
        {
            client = new HttpClient();
            client.Timeout = new TimeSpan(0, 1, 0);
            client.MaxResponseContentBufferSize = 15000;
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/image"));
        }

        static System.Net.Http.ByteArrayContent _content;
        static System.Net.Http.StreamContent sc;
        public static async Task SendData(byte[] data)
        {
            if (url == "") return;
            ///Creates the content to send from a byte array with a stream
            var cts = new System.Threading.CancellationTokenSource();


            using (ByteArrayContent content = createContent(data))
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, url))
            {
                request.Content = content;
                using(HttpResponseMessage result = await client.SendAsync(request, cts.Token))
                {
                    if (result.StatusCode == System.Net.HttpStatusCode.OK) isStreaming = true;
                    else isStreaming = false;
                    result.Content?.Dispose();
                    result.Content = null;
                }

               request.Content?.Dispose();
               request.Content = null;
            }
            /*_content = new ByteArrayContent(data);
            await client.PostAsync(url, _content);*/
        }

        private static ByteArrayContent createContent(byte[] data)
        {
            if (_content != null) _content = null;
            _content = new ByteArrayContent(data);
            return _content;
        }
    }
}
