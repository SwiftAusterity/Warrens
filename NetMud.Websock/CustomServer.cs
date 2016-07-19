using System;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using NetMud.Communication;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;

namespace NetMud.Websock
{
    public class CustomServer : Channel, IServer
    {
        static TcpListener serverSocket;
        static private string guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        public void Launch(int portNumber)
        {
            serverSocket = new TcpListener(IPAddress.Parse("127.0.0.1"), portNumber);
            serverSocket.Start();
            OnAccept();
        }

        private async void OnAccept()
        {
            Task<TcpClient> clientTask = serverSocket.AcceptTcpClientAsync();
            TcpClient client = await clientTask;

            NetworkStream stream = client.GetStream();

            //enter to an infinite cycle to be able to handle every change in stream
            while (true)
            {
                while (!stream.DataAvailable) 
                    ;

                Byte[] bytes = new Byte[client.Available];

                stream.Read(bytes, 0, bytes.Length);

                //translate bytes of request to string
                String data = Encoding.UTF8.GetString(bytes);

                if (new Regex("^GET").IsMatch(data))
                {
                    Byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + Environment.NewLine
                        + "Connection: Upgrade" + Environment.NewLine
                        + "Upgrade: websocket" + Environment.NewLine
                        + "Sec-WebSocket-Accept: " + Convert.ToBase64String(
                            SHA1.Create().ComputeHash(
                                Encoding.UTF8.GetBytes(
                                    new Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                                )
                            )
                        ) + Environment.NewLine
                        + Environment.NewLine);

                    stream.Write(response, 0, response.Length);
                }
            }
        }

        public static T[] SubArray<T>(T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);

            return result;
        }

        private static string AcceptKey(ref string key)
        {
            string longKey = key + guid;
            byte[] hashBytes = ComputeHash(longKey);

            return Convert.ToBase64String(hashBytes);
        }

        static SHA1 sha1 = SHA1CryptoServiceProvider.Create();
        private static byte[] ComputeHash(string str)
        {
            return sha1.ComputeHash(System.Text.Encoding.ASCII.GetBytes(str));
        }

        public bool Broadcast(string message)
        {
            throw new NotImplementedException();
        }

        public bool Broadcast(string message, int portNumber)
        {
            throw new NotImplementedException();
        }


        public void Shutdown(int portNumber = -1)
        {
            serverSocket.Stop();
        }

        public T GetActiveService<T>(int portNumber)
        {
            throw new NotImplementedException();
        }
    }
}