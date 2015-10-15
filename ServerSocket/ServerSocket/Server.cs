using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace ServerSocket
{
    class Server
    {

        static Socket socket;
        public static List<ClientData> clients;
        public static List<TreeData> trees = new List<TreeData>();

        public static Thread acceptThread;
        public static Thread updateThread;

        static void Main(string[] args)
        {
            Start();

            // Add trees
            for (int i = 0; i < 7; i++)
                trees.Add(new TreeData(i));

            while (true)
            {
                string command = Console.ReadLine().ToLower();

                switch (command)
                {
                    case "help":
                        {
                            Console.WriteLine(" > Available commands:");
                            Console.WriteLine("\tclear, players, save.");
                        }
                        break;
                    case "clear":
                        {
                            Console.Clear();
                            Console.WriteLine("Console cleared.");
                            break;
                        }
                    case "players":
                        {
                            int playerCount = 0;
                            string output = string.Empty;
                            foreach (ClientData client in clients)
                            {
                                if (client.online)
                                {
                                    playerCount++;
                                    output += "\n   Name: '" + client.name + "' | ID: '" + client.id + "'.";
                                }
                            }
                            if (playerCount > 0)
                                Console.WriteLine(" > There are {0} players online:" + output, playerCount);
                            else
                                Console.WriteLine(" > There are no players connected.");
                        }
                        break;
                    case "test": // JUST A TEST CASE
                        {
                            Console.WriteLine("Nothing here");
                        }
                        break;
                    default:
                        {
                            Console.WriteLine(" > This command is unknown, type 'help' to see available commands.");
                        }
                        break;
                }
            }
        }

        static void Start ()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clients = new List<ClientData>();

            IPEndPoint ip = new IPEndPoint(IPAddress.Any, 25565);
            socket.Bind(ip);
            socket.Listen(500);

            Console.WriteLine("Server started!");

            // Start listen and accepting clients
            acceptThread = new Thread(Accept);
            acceptThread.Start();

            // Start sending data updates (1000 miliseconds)
            updateThread = new Thread(PacketSender.SendDataUpdate);
            //updateThread.Start();
        }

        static void Accept()
        {            
            // Accept the connecting client
            Socket acceptedSocket = socket.Accept();

            // Receive connecting packet and get name
            byte[] buffer = new byte[acceptedSocket.SendBufferSize];
            int bufferLength = acceptedSocket.Receive(buffer);
            string name = PacketHandler.GetConnectingPlayerName(buffer);

            // Check if there is an existing player with this name
            int matchingID = -1;
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].name == name)
                    matchingID = i;
            }

            // Perform action depending on matchingID
            if (matchingID == -1)
            {
                clients.Add(new ClientData(acceptedSocket, name));
                Console.WriteLine("New player '{0}' joined the server.", name);
            }
            else
            {
                clients[matchingID].Connected(acceptedSocket);
                Console.WriteLine("Player '{0}' joined the server!", name);
            }

            Accept();
        }

        public static void ReceiveData(object id)
        {
            ClientData client = null;
            if (clients.Count > (int)id)
                client = clients[(int)id];
            else
            {
                ReceiveData(id);
                return;
            }

            while (true)
            {
                try
                {
                    // Receive packet
                    byte[] buffer = new byte[client.clientSocket.SendBufferSize];
                    int bufferLength = client.clientSocket.Receive(buffer);

                    if (bufferLength > 0)
                    {
                        PacketHandler.Handle(buffer, client.clientSocket);
                    }
                }
                catch
                {
                    // Player disconnected
                    if (clients[(int)id].online)
                        clients[(int)id].Disconnect();
                }
            }
        }

        public static ClientData GetPlayerByID (int id) // Get a player by his ID
        {
            // Loop through every player
            foreach (ClientData client in clients)
            {
                // Check if client id == id
                if (client.id == id)
                {
                    return client;
                }
            }

            return null;
        }

        public static TreeData GetTreeByID (int id) // Get tree by his ID
        {
            // Loop through every enemy
            foreach (TreeData tree in trees)
            {
                // Check if client id == id
                if (tree.id == id)
                {
                    return tree;
                }
            }

            return null;
        }
    }
    
    public class ClientData
    {
        // Client data
        public Socket clientSocket;
        public Thread clientThread;

        // Player data
        public int id;
        public bool online;
        public string name;
        public float xPos, zPos, xCurrPos, zCurrPos;

        public ClientData (Socket _clientSocket, string _name) // Create new client
        {
            // Set client data
            clientSocket = _clientSocket;
            id = Server.clients.Count;
            online = true;
            name = _name;

            // Set standard spawn position
            xPos = 250.5f;
            zPos = 250.5f;
            xCurrPos = 250.5f;
            zCurrPos = 250.5f;
            
            // Start a thread for this client
            clientThread = new Thread(Server.ReceiveData);
            clientThread.Start(id);

            // Send packet to show the user that he connected
            PacketSender.PlayerConnected("Welcome to the server " + name + "!", this, clientSocket);
        }
         
        public void Connected (Socket _clientSocket) // Client rejoined
        {
            // Set client data6
            clientSocket = _clientSocket;
            online = true;

            // Start a thread for this client
            clientThread = new Thread(Server.ReceiveData);
            clientThread.Start(id);

            // Send packet to show the user that he connected
            PacketSender.PlayerConnected("Welcome back " + name + "!", this, clientSocket);
        }

        public void Disconnect ()
        {
            // Set client data
            clientSocket.Close();
            clientSocket = null;
            online = false;

            // Send client disconnect call to clients
            PacketSender.SendPlayerDisconnect(this);

            // Convert current pos to wanted pos
            xPos = ConvertToWalkPos(xCurrPos);
            zPos = ConvertToWalkPos(zCurrPos);
            
            // Write to console
            Console.WriteLine("Player '{0}' disconnected.", name);

            // Stop thread
            clientThread.Abort();
        }

        public float ConvertToWalkPos(float value)
        {
            double result = 0;
            if (Math.Round(value, 0) > value)
                result = Math.Round(value, 0) - .5f;
            else
                result = Math.Round(value, 0) + .5f;

            return (float)result;
        }
    }

    public class TreeData
    {
        public int id;
        public int chopperID;
        public bool isBeingChopped;
        public bool isChopped;

        public int reupTime;

        public TreeData (int iD) // Create new tree
        {
            id = iD;
            isBeingChopped = false;
            isChopped = false;
        }
    }
}
