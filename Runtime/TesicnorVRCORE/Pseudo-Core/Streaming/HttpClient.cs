using System;
using System.IO;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;


namespace StreamingCSharp
{
    public class HttpClient_Custom
    {
        public static string url = "http://192.168.20.55:8080";
        private static System.Net.Http.HttpClient client;
        private static byte[] content;

        public static void IntializeClient()
        {
            client = new HttpClient(new HttpClientHandler(), false);
            client.Timeout = new TimeSpan(0, 1, 0);
            client.MaxResponseContentBufferSize = 2048;
            client.BaseAddress = new Uri(url);
        }

        public static async void  AskForData()
        {
            var headers = client.DefaultRequestHeaders;

            //Checking correct header
            string header = "ie";
            if (!headers.UserAgent.TryParseAdd(header))
            {
                throw new Exception("Invalid Header Value: " + header);
            }

            //Setting the uri which we are going to connect
            Uri uri = new Uri(url);
            System.Net.Http.HttpResponseMessage response = new System.Net.Http.HttpResponseMessage();
            byte[] responseBody;

            //Try to get the data from the response
            try
            {
                //Wait till the response is ready
                response = await client.GetAsync(uri, System.Threading.CancellationToken.None);
                //Make sure everything is ok
                response.EnsureSuccessStatusCode();

                //Copy the content to an empty stream
                Stream stream = Stream.Null;
                response.Content.CopyToAsync(stream).Wait();
                responseBody = new byte[stream.Length];
                
                //Copy the stream to an empty byte array
                stream.Read(responseBody, 0, responseBody.Length);
                content = responseBody;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public static byte[] GetContent()
        {
            return content;
        }

        public static Image GetContent_IMG()
        {
            return ImageByteConverter.BytesToImage(content);
        }

        public static async Task SendData(byte[] data)
        {
            //Creates the content to send from a byte array with a stream
            System.Net.Http.ByteArrayContent content = new ByteArrayContent(data);

            //Post the bytes
            await client.PostAsync("http://192.168.20.55:8080/", content);
        }
    }
}
