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

        public static void Handle (byte[] packet, Socket socket) // Decide with function to call
        {
            ushort packetType = BitConverter.ToUInt16(packet, 0); // Get packettype
            
            switch (packetType) // Switch to get correct function
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
                    ReceiveTreeState(packet);
                    break;
            }
        }

        public static string GetConnectingPlayerName (byte[] packet) // This packet gets the name of a connecting player
        {
            // Convert data
            ushort nameLength = BitConverter.ToUInt16(packet, 0);
            return Encoding.ASCII.GetString(packet, 2, nameLength);
        }

        public static void PlayerDisconnect (byte[] packet)
        {
            int id = BitConverter.ToInt16(packet, 2); // Player ID
            PacketSender.PlayerDisconnected(id);
        }

        public static void ReceiveWantedPosition (byte[] packet)
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
                Console.WriteLine("Failed receiving wanted position.");
            }
        }

        public static void ReceiveCurrentPosition (byte[] packet)
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
                Console.WriteLine("Failed receiving current position.");
            }
        }

        public static void ReceiveTreeState (byte[] packet)
        {
            // Convert data
            int playerID = BitConverter.ToUInt16(packet, 2);
            int treeID = BitConverter.ToUInt16(packet, 4);
            int treeState = BitConverter.ToUInt16(packet, 6);

            // Find and set tree data
            TreeData tree = Server.GetTreeByID(treeID);
            if (treeState == 0)
            {
                tree.isChopped = false;
                tree.isBeingChopped = false;
                tree.chopperID = -1;
            }
            if (treeState == 1)
            {
                tree.isChopped = false;
                tree.isBeingChopped = true;
                tree.chopperID = playerID;
            }
            if (treeState == 2)
            {
                tree.isChopped = true;
                tree.isBeingChopped = false;
                tree.chopperID = -1;
            }

            // Send packet to clients that tree is being chopped
            PacketSender.SendTreeState(treeID, treeState, playerID);
            Console.WriteLine("Player {0} set state of tree {1} to {2}", playerID, treeID, treeState);
        }
    }
}
