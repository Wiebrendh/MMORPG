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

        packet.AddRange(BitConverter.GetBytes((ushort)2)); // Packet type
        packet.AddRange(BitConverter.GetBytes((ushort)game.playerID)); // Player id
        packet.AddRange(BitConverter.GetBytes((double)pos.x)); // Player pos x
        packet.AddRange(BitConverter.GetBytes((double)pos.y)); // Player pos z

        // Send packet
        socket.socket.Send(packet.ToArray(), SocketFlags.None);
    }

    public void SendTreeState (int treeID, int state)
    {
        List<byte> packet = new List<byte>();

        packet.AddRange(BitConverter.GetBytes((ushort)3)); // Packet type
        packet.AddRange(BitConverter.GetBytes((ushort)game.playerID)); // Player id
        packet.AddRange(BitConverter.GetBytes((ushort)treeID)); // Tree id
        packet.AddRange(BitConverter.GetBytes((ushort)state)); // Tree id

        // Send packet
        socket.socket.Send(packet.ToArray(), SocketFlags.None);
    }
}