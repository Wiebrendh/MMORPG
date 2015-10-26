using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
using System.Net.Sockets;

public class PacketSender : MonoBehaviour
{

    public Game game;
    public ClientSocket socket;

    public void Disconnect(int id)
    {
        List<byte> packet = new List<byte>();

        packet.AddRange(BitConverter.GetBytes((ushort)0)); // Packet type
        packet.AddRange(BitConverter.GetBytes((ushort)id)); // Player ID

        // Send packet
        socket.socket.Send(packet.ToArray(), SocketFlags.None);

        // Disconnect client
        socket.socket.Shutdown(SocketShutdown.Both);
        socket.socket.Close();
    }

    public void SendWantedPosition (Vector2 pos) // Send packet with player transforms
	{
		List<byte> packet = new List<byte>();

        packet.AddRange(BitConverter.GetBytes((ushort)1)); // Packet type
        packet.AddRange(BitConverter.GetBytes((ushort)game.playerID)); // Player id
        packet.AddRange(BitConverter.GetBytes((double)pos.x)); // Player pos x
        packet.AddRange(BitConverter.GetBytes((double)pos.y)); // Player pos z

        // Send packet
        socket.socket.Send(packet.ToArray(), SocketFlags.None);
	}

    public void SendCurrentPos (Vector2 pos)
    {
        List<byte> packet = new List<byte>();

        // Add data
        packet.AddRange(BitConverter.GetBytes((ushort)2)); // Packet type
        packet.AddRange(BitConverter.GetBytes((ushort)game.playerID)); // Player id
        packet.AddRange(BitConverter.GetBytes((double)pos.x)); // Player pos x
        packet.AddRange(BitConverter.GetBytes((double)pos.y)); // Player pos z

        // Send packet
        socket.socket.Send(packet.ToArray(), SocketFlags.None);
    }

    public void SendTextMessage (string message)
    {
        List<byte> packet = new List<byte>();

        // Add data
        packet.AddRange(BitConverter.GetBytes((ushort)3)); // Packet type
        packet.AddRange(BitConverter.GetBytes((ushort)game.playerName.Length)); // Player name length
        packet.AddRange(BitConverter.GetBytes((ushort)message.Length)); // Player message length
        packet.AddRange(Encoding.ASCII.GetBytes(game.playerName)); // Player name
        packet.AddRange(Encoding.ASCII.GetBytes(message)); // Player message
        
        // Send packet
        socket.socket.Send(packet.ToArray(), SocketFlags.None);
    }

    public void SendLevelsRequest ()
    {
        List<byte> packet = new List<byte>();

        packet.AddRange(BitConverter.GetBytes((ushort)4)); // Packet type
        packet.AddRange(BitConverter.GetBytes((ushort)game.playerID)); // Player id

        // Send packet
        socket.socket.Send(packet.ToArray(), SocketFlags.None);
    }

    public void SendHarvestObject (int type, int id)
    {
        List<byte> packet = new List<byte>();

        packet.AddRange(BitConverter.GetBytes((ushort)5)); // Packet type
        packet.AddRange(BitConverter.GetBytes((ushort)game.playerID)); // Player id
        packet.AddRange(BitConverter.GetBytes((ushort)type)); // Object type
        packet.AddRange(BitConverter.GetBytes((ushort)id)); // Object id

        // Send packet
        socket.socket.Send(packet.ToArray(), SocketFlags.None);
    }
}