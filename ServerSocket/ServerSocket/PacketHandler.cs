using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerSocket
{
    public static class PacketHandler
    {

        public static void Handle (byte[] packet) // Decide with function to call
        {
            // Get packettype
            ushort packetType = BitConverter.ToUInt16(packet, 0);

            // Switch to get correct function
            switch (packetType)
            {
                case 0:
                    PlayerDisconnect(packet);
                    break;
                case 1:
                    ReceiveWantedPosition(packet);
                    break;
                case 2:
                    ReceiveCurrentPosition(packet);
                    break;
                case 3:
                    ReceiveTextMessage(packet);
                    break;
                case 4:
                    ReceiveLevelRequest(packet);
                    break;
                case 5:
                    ReceiveHarvestObject(packet);
                    break;
            }
        }

        public static string GetConnectingPlayerName (byte[] packet) // This packet gets the name of a connecting player
        {
            // Convert data
            ushort nameLength = BitConverter.ToUInt16(packet, 0);
            return Encoding.ASCII.GetString(packet, 2, nameLength);
        }

        public static void PlayerDisconnect (byte[] packet) // Receive player disconnect
        {
            // Convert data, store in variable
            int id = BitConverter.ToInt16(packet, 2);

            // Send data to other players
            PacketSender.PlayerDisconnected(id);
        }

        public static void ReceiveWantedPosition (byte[] packet) // Receive wanted position of player
        {
            int playerID = BitConverter.ToInt16(packet, 2);
            
            try
            {
                // Convert position data
                Server.clients[playerID].xPos = (float)BitConverter.ToDouble(packet, 4);
                Server.clients[playerID].zPos = (float)BitConverter.ToDouble(packet, 12);
                PacketSender.SendPlayerPosition(playerID);
            }
            catch
            {
                // Log error if there is one
                Console.WriteLine("Failed receiving wanted position.");
            }
        }

        public static void ReceiveCurrentPosition (byte[] packet) // Receive current position of player
        {
            int playerID = BitConverter.ToInt16(packet, 2);

            try
            {
                // Convert position data
                Server.clients[playerID].xCurrPos = (float)BitConverter.ToDouble(packet, 4);
                Server.clients[playerID].zCurrPos = (float)BitConverter.ToDouble(packet, 12);
            }
            catch
            {
                // Debug error if there is one
                Console.WriteLine("Failed receiving current position.");
            }
        }

        public static void ReceiveTextMessage(byte[] packet) // Receive text message send by player
        {
            // Convert data store in variables
            int playerNameLength = BitConverter.ToInt16(packet, 2);
            int playerMessageLength = BitConverter.ToInt16(packet, 4);
            string playerName = Encoding.ASCII.GetString(packet, 6, playerNameLength);
            string playerMessage = Encoding.ASCII.GetString(packet, 6 + playerNameLength, playerMessageLength);

            // Send packet to clients with this message
            PacketSender.SendTextMessage(false, null, playerName, playerMessage);
        }

        public static void ReceiveLevelRequest (byte[] packet) // Receive request from player for stats
        {
            // Convert data
            int playerID = BitConverter.ToInt16(packet, 2);

            // Send packet to clients that tree is being chopped
            Server.clients[playerID].levels.SendData();
        }

        public static void ReceiveHarvestObject (byte[] packet) // Receive request from player to harvest a object
        {
            // Convert data
            int playerID = BitConverter.ToInt16(packet, 2);
            int objectType = BitConverter.ToInt16(packet, 4);
            int objectID = BitConverter.ToInt16(packet, 6);
            
            // Use this data
            switch (objectType)
            {
                case 0:
                    {
                        TreeData tree = (TreeData)Server.worldObjects[0][objectID];

                        // If you can chop the tree
                        if (Math.Round(tree.chopTimeLeft, 1) == 0f && Math.Round(tree.reupTimeLeft, 1) == 0)
                        {
                            tree.chopperClient = Server.clients[playerID];
                            tree.chopTimeLeft = 5;
                        }
                        else
                            PacketSender.SendTextMessage(true, Server.clients[playerID], string.Empty, "This tree is already being chopped or it is down.");
                    }                    
                    break;
            }

            // Send packet to clients that tree is being chopped
            Server.clients[playerID].levels.SendData();
        }

    }
}
