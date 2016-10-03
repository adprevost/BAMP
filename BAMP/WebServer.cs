using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace BAMP
{
    public class WebServer
    {
        private const uint BufferSize = 8192;

        public void Start()
        {
            StreamSocketListener listener = new StreamSocketListener();

            listener.BindServiceNameAsync("80");

            listener.ConnectionReceived += async (sender, args) =>
            {
                StringBuilder request = new StringBuilder();
                using (IInputStream input = args.Socket.InputStream)
                {
                    byte[] data = new byte[BufferSize];
                    IBuffer buffer = data.AsBuffer();
                    uint dataRead = BufferSize;
                    while (dataRead == BufferSize)
                    {
                        await input.ReadAsync(buffer, BufferSize, InputStreamOptions.Partial);
                        request.Append(Encoding.UTF8.GetString(data, 0, data.Length));
                        dataRead = buffer.Length;
                    }
                }

                using (IOutputStream output = args.Socket.OutputStream)
                {
                    using (Stream response = output.AsStreamForWrite())
                    {
                        byte[] bodyArray = null;

                        if (request.ToString().Contains("api/status"))
                        {
                            bodyArray = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(MainPage.Doors));
                        }
                        else
                        {
                            StringBuilder body = new StringBuilder();
                            foreach (var door in MainPage.Doors)
                            {
                                body.AppendLine($"{door.Name}: { (door.InUse ? "In Use" : "Free") }");
                            }
                            string html = body.Replace("\r\n", "\r\n<br />").ToString();
                            bodyArray = Encoding.UTF8.GetBytes($"<html><head><meta http-equiv='refresh' content='5' /></head><body>{html}</body></html>");
                        }

                        var bodyStream = new MemoryStream(bodyArray);

                        var header = "HTTP/1.1 200 OK\r\n" +
                                    $"Content-Length: {bodyStream.Length}\r\n" +
                                        "Connection: close\r\n\r\n";

                        byte[] headerArray = Encoding.UTF8.GetBytes(header);
                        await response.WriteAsync(headerArray, 0, headerArray.Length);
                        await bodyStream.CopyToAsync(response);
                        await response.FlushAsync();
                    }
                }
            };
        }
    }
}