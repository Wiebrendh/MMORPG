using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;

namespace ServerSocket
{
    public static class PacketSender
    {
        
        public static void PlayerConnected (string message, ClientData data, Socket client)
        {
            List<byte> packet = new List<byte>();

            packet.AddRange(BitConverter.GetBytes((ushort)0)); // Packettype
            packet.AddRange(BitConverter.GetBytes((ushort)data.id)); // Player ID
            packet.AddRange(BitConverter.GetBytes((ushort)message.Length)); // Message for player length
            packet.AddRange(Encoding.ASCII.GetBytes(message)); // Message for player 

            // Add player pos
            packet.AddRange(BitConverter.GetBytes((double)data.xPos)); // Player xPos
            packet.AddRange(BitConverter.GetBytes((double)data.zPos)); // Player zPos

            // Add every player that is online, his wanted position and his current position to player packet
            int amountOfPlayers = 0;
            List<byte> playerPacket = new List<byte>();
            foreach (ClientData player in Server.clients)
            {
                if (player.online && player != data)
                {
                    amountOfPlayers++;
                    playerPacket.AddRange(BitConverter.GetBytes((ushort)player.id));
                    playerPacket.AddRange(BitConverter.GetBytes((double)player.xCurrPos));
                    playerPacket.AddRange(BitConverter.GetBytes((double)player.zCurrPos));
                    playerPacket.AddRange(BitConverter.GetBytes((double)player.xPos));
                    playerPacket.AddRange(BitConverter.GetBytes((double)player.zPos));
                }
            }

            // Add amountOfPlayer and playerPacket to packet
            packet.AddRange(BitConverter.GetBytes((ushort)amountOfPlayers));
            packet.AddRange(playerPacket);

            // Send packet
            client.Send(packet.ToArray(), SocketFlags.None);

            // Send data to all connected clients that this client joined
            PacketSender.SendPlayerConnect(data);
        }

        public static void PlayerDisconnected (int id)
        {
            List<byte> packet = new List<byte>();

            packet.AddRange(BitConverter.GetBytes((ushort)1));

            Server.clients[id].clientSocket.Send(packet.ToArray(), SocketFlags.None);
            Server.clients[id].Disconnect(); // Start disconnect function
        }

        public static void SendDataUpdate()
        {
            List<byte> packet = new List<byte>();

            packet.AddRange(BitConverter.GetBytes((ushort)2)); // Packet type

            // Send data to every client, if connected
            foreach (ClientData client in Server.clients)
            {
                if (client.online && client.clientSocket.Connected)
                {
                    client.clientSocket.Send(packet.ToArray(), 0, packet.Count, SocketFlags.None); 
                }
            }

            // Resend update after 1000 miliseconds
            Thread.Sleep(1000);
            SendDataUpdate();
        }

        public static void SendPlayerConnect (ClientData player)
        {
            List<byte> packet = new List<byte>();
            
            packet.AddRange(BitConverter.GetBytes((ushort)1)); // Packet type
            packet.AddRange(BitConverter.GetBytes((ushort)player.id)); // Client id
            packet.AddRange(BitConverter.GetBytes((double)player.xPos)); // Client x pos
            packet.AddRange(BitConverter.GetBytes((double)player.zPos)); // Client z pos

            // Send data to every client, if connected
            foreach (ClientData client in Server.clients)
            {
                if (client.online && client.clientSocket.Connected && client != player)
                {
                    client.clientSocket.Send(packet.ToArray(), 0, packet.Count, SocketFlags.None);
                }
            }
        }

        public static void SendPlayerDisconnect (ClientData player)
        {
            List<byte> packet = new List<byte>();

            packet.AddRange(BitConverter.GetBytes((ushort)2)); // Packet type
            packet.AddRange(BitConverter.GetBytes((ushort)player.id)); // Client id

            // Send data to every client, if connected
            foreach (ClientData client in Server.clients)
            {
                if (client.online && client.clientSocket.Connected && client != player)
                {
                    client.clientSocket.Send(packet.ToArray(), 0, packet.Count, SocketFlags.None);
                }
            }
        }

        public static void SendPlayerPosition (int playerID)
        {
            List<byte> packet = new List<byte>();

            packet.AddRange(BitConverter.GetBytes((ushort)3)); // Packet type
            packet.AddRange(BitConverter.GetBytes((ushort)playerID)); // Client id
            packet.AddRange(BitConverter.GetBytes((double)Server.clients[playerID].xPos)); // Client x pos
            packet.AddRange(BitConverter.GetBytes((double)Server.clients[playerID].zPos)); // Client z pos

            // Send data to every client, if connected
            foreach (ClientData client in Server.clients)
            {
                if (client.online && client.clientSocket.Connected)
                {
                    client.clientSocket.Send(packet.ToArray(), 0, packet.Count, SocketFlags.None);
                }
            }
        }

        public static void SendTextMessage(string sender, string message)
        {
            List<byte> packet = new List<byte>();

            packet.AddRange(BitConverter.GetBytes((ushort)4)); // Packet type
            packet.AddRange(BitConverter.GetBytes((ushort)sender.Length)); // Player name length
            packet.AddRange(BitConverter.GetBytes((ushort)message.Length)); // Player message length
            packet.AddRange(Encoding.ASCII.GetBytes(sender)); // Player name
            packet.AddRange(Encoding.ASCII.GetBytes(message)); // Player message

            // Send data to every client, if connected
            foreach (ClientData client in Server.clients)
            {
                if (client.online && client.clientSocket.Connected)
                {
                    client.clientSocket.Send(packet.ToArray(), 0, packet.Count, SocketFlags.None);
                }
            }
        }

        public static void SendLevelUpdate(ClientData client, byte[] packet)
        {
            // Send packet to client, if connected
            if (client.online && client.clientSocket.Connected)
            {
                client.clientSocket.Send(packet, 0, packet.Length, SocketFlags.None);
            }
        }
    }
}