using NetMud.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetMud.Telnet
{
    public class Server : Channel, IServer
    {
        private static Socket serverSocket;
        private static byte[] data = new byte[dataSize];
        private static bool newClients = true;
        private const int dataSize = 1024;
        private static Dictionary<Socket, Client> clientList = new Dictionary<Socket, Client>();

        private static void backgroundThread()
        {
            while (true)
            {
                string Input = Console.ReadLine();

                if (Input == "clients")
                {
                    if (clientList.Count == 0) continue;
                    int clientNumber = 0;
                    foreach (KeyValuePair<Socket, Client> client in clientList)
                    {
                        Client currentClient = client.Value;
                        clientNumber++;
                        Console.WriteLine(string.Format("Client #{0} (From: {1}:{2}, ECurrentState: {3}, Connection time: {4})", clientNumber,
                            currentClient.remoteEndPoint.Address.ToString(), currentClient.remoteEndPoint.Port, currentClient.clientState, currentClient.connectedAt));
                    }
                }

                if (Input.StartsWith("kill"))
                {
                    string[] _Input = Input.Split(' ');
                    int clientID = 0;
                    try
                    {
                        if (Int32.TryParse(_Input[1], out clientID) && clientID >= clientList.Keys.Count)
                        {
                            int currentClient = 0;
                            foreach (Socket currentSocket in clientList.Keys.ToArray())
                            {
                                currentClient++;
                                if (currentClient == clientID)
                                {
                                    currentSocket.Shutdown(SocketShutdown.Both);
                                    currentSocket.Close();
                                    clientList.Remove(currentSocket);
                                    Console.WriteLine("Client has been disconnected and cleared up.");
                                }
                            }
                        }
                        else { Console.WriteLine("Could not kick client: invalid client number specified."); }
                    }
                    catch { Console.WriteLine("Could not kick client: invalid client number specified."); }
                }

                if (Input == "killall")
                {
                    int deletedClients = 0;
                    foreach (Socket currentSocket in clientList.Keys.ToArray())
                    {
                        currentSocket.Shutdown(SocketShutdown.Both);
                        currentSocket.Close();
                        clientList.Remove(currentSocket);
                        deletedClients++;
                    }

                    Console.WriteLine("{0} clients have been disconnected and cleared up.", deletedClients);
                }

                if (Input == "lock") { newClients = false; Console.WriteLine("Refusing new connections."); }
                if (Input == "unlock") { newClients = true; Console.WriteLine("Accepting new connections."); }
            }
        }

        private static void AcceptConnection(IAsyncResult result)
        {
            if (!newClients) return;
            Socket oldSocket = (Socket)result.AsyncState;
            Socket newSocket = oldSocket.EndAccept(result);
            Client client = new Client((IPEndPoint)newSocket.RemoteEndPoint, DateTime.Now, EClientState.NotLogged);
            clientList.Add(newSocket, client);
            Console.WriteLine("Client connected. (From: " + string.Format("{0}:{1}", client.remoteEndPoint.Address.ToString(), client.remoteEndPoint.Port) + ")");
            string output = "-- TELNET TEST SERVER --\n\r\n\r";
            output += "Please input your password:\n\r";
            client.clientState = EClientState.Logging;
            byte[] message = Encoding.ASCII.GetBytes(output);
            newSocket.BeginSend(message, 0, message.Length, SocketFlags.None, new AsyncCallback(SendData), newSocket);
            serverSocket.BeginAccept(new AsyncCallback(AcceptConnection), serverSocket);
        }

        private static void SendData(IAsyncResult result)
        {
            try
            {
                Socket clientSocket = (Socket)result.AsyncState;
                clientSocket.EndSend(result);
                clientSocket.BeginReceive(data, 0, dataSize, SocketFlags.None, new AsyncCallback(ReceiveData), clientSocket);
            }
            catch { }
        }

        private static void ReceiveData(IAsyncResult result)
        {
            try
            {
                Socket clientSocket = (Socket)result.AsyncState;
                Client client;
                clientList.TryGetValue(clientSocket, out client);
                int received = clientSocket.EndReceive(result);
                if (received == 0)
                {
                    clientSocket.Close();
                    clientList.Remove(clientSocket);
                    serverSocket.BeginAccept(new AsyncCallback(AcceptConnection), serverSocket);
                    Console.WriteLine("Client disconnected. (From: " + string.Format("{0}:{1}", client.remoteEndPoint.Address.ToString(), client.remoteEndPoint.Port) + ")");
                    return;
                }

                Console.WriteLine("Received '{0}' (From: {1}:{2})", BitConverter.ToString(data, 0, received), client.remoteEndPoint.Address.ToString(), client.remoteEndPoint.Port);

                // 0x2E & 0X0D => return/intro
                if (data[0] == 0x2E && data[1] == 0x0D && client.commandIssued.Length == 0)
                {
                    string currentCommand = client.commandIssued;
                    Console.WriteLine(string.Format("Received '{0}' while EClientStatus '{1}' (From: {2}:{3})", currentCommand, client.clientState.ToString(), client.remoteEndPoint.Address.ToString(), client.remoteEndPoint.Port));
                    client.commandIssued = "";
                    byte[] message = Encoding.ASCII.GetBytes("\u001B[1J\u001B[H" + HandleCommand(clientSocket, currentCommand));
                    clientSocket.BeginSend(message, 0, message.Length, SocketFlags.None, new AsyncCallback(SendData), clientSocket);
                }

                else if (data[0] == 0x0D && data[1] == 0x0A)
                {
                    string currentCommand = client.commandIssued;
                    Console.WriteLine(string.Format("Received '{0}' (From: {1}:{2}", currentCommand, client.remoteEndPoint.Address.ToString(), client.remoteEndPoint.Port));
                    client.commandIssued = "";
                    byte[] message = Encoding.ASCII.GetBytes("\u001B[1J\u001B[H" + HandleCommand(clientSocket, currentCommand));
                    clientSocket.BeginSend(message, 0, message.Length, SocketFlags.None, new AsyncCallback(SendData), clientSocket);
                }

                else
                {
                    // 0x08 => remove character
                    if (data[0] == 0x08)
                    {
                        if (client.commandIssued.Length > 0)
                        {
                            client.commandIssued = client.commandIssued.Substring(0, client.commandIssued.Length - 1);
                            byte[] message = Encoding.ASCII.GetBytes("\u0020\u0008");
                            clientSocket.BeginSend(message, 0, message.Length, SocketFlags.None, new AsyncCallback(SendData), clientSocket);
                        }
                        else
                        {
                            clientSocket.BeginReceive(data, 0, dataSize, SocketFlags.None, new AsyncCallback(ReceiveData), clientSocket);
                        }
                    }
                    // 0x7F => delete character
                    else if (data[0] == 0x7F)
                    {
                        clientSocket.BeginReceive(data, 0, dataSize, SocketFlags.None, new AsyncCallback(ReceiveData), clientSocket);
                    }
                    else
                    {
                        string currentCommand = client.commandIssued;
                        client.commandIssued += Encoding.ASCII.GetString(data, 0, received);
                        clientSocket.BeginReceive(data, 0, dataSize, SocketFlags.None, new AsyncCallback(ReceiveData), clientSocket);
                    }
                }
            }
            catch { }
        }

        private static string HandleCommand(Socket clientSocket, string Input)
        {
            string Output = "-- TELNET TEST SERVER --\n\r\n\r";
            byte[] dataInput = Encoding.ASCII.GetBytes(Input);
            Client client;
            clientList.TryGetValue(clientSocket, out client);
            /*if (client.clientState == EClientState.NotLogged)
            {
                Console.WriteLine("Client not logged in, marking login operation in progress...");
                client.clientState = EClientState.Logging;
                Output += "Please input your password:\n\r";
            }*/
            if (client.clientState == EClientState.Logging)
            {
                if (Input == "1337")
                {
                    Console.WriteLine("Client has logged in (correct password), marking as logged...");
                    client.clientState = EClientState.LoggedIn;
                    Output += "Logged successfully.\n\r";
                }
                else
                {
                    Console.WriteLine("Client login failed (incorrect password).");
                    Output += "Incorrect password. Please input your password: ";
                }
            }
            if (client.clientState == EClientState.LoggedIn)
            {
                if (Input == "test")
                {
                    Output += "Hello there.\n\r";
                }
                if (Input == "getrekt")
                {
                    return Output;
                }
                else
                {
                    Output += "Please enter a valid command:\n\r";
                }
            }
            return Output;
        }

        public bool Broadcast(string message)
        {
            throw new NotImplementedException();
        }

        public bool Broadcast(string message, int portNumber)
        {
            throw new NotImplementedException();
        }

        public void Launch(int portNumber)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, portNumber);

            new Thread(new ThreadStart(backgroundThread)) { IsBackground = false }.Start();

            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(endPoint);
            serverSocket.Listen(0);
            serverSocket.BeginAccept(new AsyncCallback(AcceptConnection), serverSocket);
        }

        public void Shutdown(int portNumber = -1)
        {
            throw new NotImplementedException();
        }

        public T GetActiveService<T>(int portNumber)
        {
            throw new NotImplementedException();
        }
    }
}
