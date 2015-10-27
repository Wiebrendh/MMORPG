using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;

namespace ServerSocket
{
    class Server
    {

        static Socket socket;
        public static List<ClientData> clients;
        public static List<List<object>> worldObjects = new List<List<object>>();

        public static Thread acceptThread;
        public static Thread objectsThread;

        static void Main(string[] args)
        {
            Start();

            // Add trees
            worldObjects.Add(new List<object>());
            for (int i = 0; i < 9; i++)
                worldObjects[0].Add(new TreeData(i));

            while (true)
            {
                // Read command in console
                string command = Console.ReadLine().ToLower();

                // Use command to do stuff
                switch (command)
                {
                    case "help": // Debug available commands
                        {
                            Console.WriteLine(" > Available commands:");
                            Console.WriteLine("\tclear, players, save.");
                        }
                        break;
                    case "clear": // Clear the console
                        {
                            Console.Clear();
                            Console.WriteLine("Console cleared.");
                            break;
                        }
                    case "players": // Debug all the player
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
                    default: // Default case, when command is not recognized
                        {
                            Console.WriteLine(" > This command is unknown, type 'help' to see available commands.");
                        }
                        break;
                }
            }
        }

        static void Start ()
        {
            // Create socket
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clients = new List<ClientData>();

            // Assign data to socket
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, 25565);
            socket.Bind(ip);
            socket.Listen(500);

            // Debug server started
            Console.WriteLine("Server started!");

            // Start listen and accepting clients
            acceptThread = new Thread(Accept);
            acceptThread.Start();

            // Start world objects thread
            objectsThread = new Thread(StartWorldObjectsThread);
            objectsThread.Start();
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

        public static void ReceiveData(object data)
        {
            // Get client
            ClientData client = (ClientData)data;

            while (true)
            {
                try
                {
                    // Receive packet
                    byte[] buffer = new byte[client.clientSocket.SendBufferSize];
                    int bufferLength = client.clientSocket.Receive(buffer);

                    if (bufferLength > 0)
                    {
                        PacketHandler.Handle(buffer);
                    }
                }
                catch { }
            }
        }

        public static void StartWorldObjectsThread ()
        {
            // Restart function after 100 miliseconds
            System.Timers.Timer timer = new System.Timers.Timer(100);
            timer.Elapsed += new ElapsedEventHandler(WorldObjectsThread);
            timer.Enabled = true;
        }

        public static void WorldObjectsThread (object sender, ElapsedEventArgs e)
        {
            // Control trees
            foreach (TreeData tree in worldObjects[0])
            {
                // Check for trees that are being chopped
                if (tree.chopTimeLeft >= .1f)
                {
                    tree.chopTimeLeft -= .1f;
                    tree.chopTimeLeft = (float)Math.Round(tree.chopTimeLeft, 1);
                    
                    // If tree is chopped down
                    if (Math.Round(tree.chopTimeLeft, 1) == 0f)
                    {
                        tree.chopTimeLeft = 0;
                        tree.reupTimeLeft = 10;
                        tree.chopperClient.levels.AddXP(3, 25);
                        PacketSender.SendObjectState(0, tree.id, false);          
                    }
                }

                // Check for trees that are down
                if (tree.reupTimeLeft >= .1f)
                {
                    tree.reupTimeLeft -= .1f;
                    tree.reupTimeLeft = (float)Math.Round(tree.reupTimeLeft, 1);
                    
                    // If tree is chopped down
                    if (Math.Round(tree.reupTimeLeft, 1) == 0f)
                    {
                        tree.reupTimeLeft = 0;
                        PacketSender.SendObjectState(0, tree.id, true);
                    }
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
            foreach (TreeData tree in worldObjects[0])
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
        public ClientLevels levels;

        public ClientData (Socket _clientSocket, string _name) // Create new client
        {
            // Set client data
            clientSocket = _clientSocket;
            id = Server.clients.Count;
            online = true;
            name = _name;

            // Set standard spawn position
            xPos = 254.5f;
            zPos = 247.5f;
            xCurrPos = 254.5f;
            zCurrPos = 247.5f;

            // Start a thread for this client
            clientThread = new Thread(Server.ReceiveData);
            clientThread.Start(this);

            // Send packet to show the user that he connected
            PacketSender.PlayerConnected("Welcome to the server " + name, this, clientSocket);

            // Create client levels
            levels = new ClientLevels(this);
        }
         
        public void Connected (Socket _clientSocket) // Client rejoined
        {
            // Set client data
            clientSocket = _clientSocket;
            online = true;

            // Start a thread for this client
            clientThread = new Thread(Server.ReceiveData);
            clientThread.Start(this);

            // Send packet to show the user that he connected
            PacketSender.PlayerConnected("Welcome back " + name, this, clientSocket);

            // Ask for level update
            levels.SendData();
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

    public class ClientLevels
    {

        public ClientData client;

        public List<int> neededXP = new List<int>();

        public List<int> levels = new List<int>();
        public List<int> xp = new List<int>();

        public ClientLevels (ClientData _client)
        {
            // Add client to this object
            client = _client;

            // Add xp ranges
            neededXP.AddRange(new List<int>() { 83, 174, 276, 388, 512, 650, 801, 969, 1154, 1358, 1584, 1833, 2107, 2411, 2746, 3115, 3523, 3973, 4470 }); // Done till lvl 20

            // Add levels and xp
            for (int i = 0; i < 4; i++)
            {
                levels.Add(1);
                xp.Add(0);
            }
        }

        public void AddXP (int skill, int amount)
        {
            // Add xp and check for level up
            xp[skill] += amount;
            if (xp[skill] > neededXP[levels[skill] - 1])
            {
                levels[skill]++;
            }
            SendData();
        }

        public void SendData ()
        {
            // Send level update to client
            List<byte> packet = new List<byte>();

            packet.AddRange(BitConverter.GetBytes((ushort)5)); // Packet type

            // Add current level
            packet.AddRange(BitConverter.GetBytes((ushort)levels[0])); // Attack level
            packet.AddRange(BitConverter.GetBytes((ushort)levels[1])); // Strength level
            packet.AddRange(BitConverter.GetBytes((ushort)levels[2])); // Defence level
            packet.AddRange(BitConverter.GetBytes((ushort)levels[3])); // Woodcutting level

            // Add current xp
            packet.AddRange(BitConverter.GetBytes((int)xp[0])); // Attack xp
            packet.AddRange(BitConverter.GetBytes((int)xp[1])); // Strength xp
            packet.AddRange(BitConverter.GetBytes((int)xp[2])); // Defence xp
            packet.AddRange(BitConverter.GetBytes((int)xp[3])); // Woodcutting xp

            // Add needed xp
            packet.AddRange(BitConverter.GetBytes((int)neededXP[levels[0] - 1])); // Attack needed xp
            packet.AddRange(BitConverter.GetBytes((int)neededXP[levels[1] - 1])); // Strength needed xp
            packet.AddRange(BitConverter.GetBytes((int)neededXP[levels[2] - 1])); // Defence needed xp
            packet.AddRange(BitConverter.GetBytes((int)neededXP[levels[3] - 1])); // Woodcutting needed xp

            // Send packet function
            PacketSender.SendStatsUpdate(client, packet.ToArray());
        }
    }

    public class TreeData
    {
        public int id;
        public ClientData chopperClient;

        public float chopTimeLeft;
        public float reupTimeLeft;

        public TreeData (int _id) // Create new tree
        {
            id = _id;
        }
    }

}