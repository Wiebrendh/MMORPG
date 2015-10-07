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

        packet.AddRange(BitConverter.GetBytes((ushort)1)); // Packet type
        packet.AddRange(BitConverter.GetBytes((ushort)id)); // Player ID

        // Send packet
        socket.socket.Send(packet.ToArray(), SocketFlags.None);

        // Disconnect client
        socket.socket.Shutdown(SocketShutdown.Both);
        socket.socket.Close();
    }

    public void SendWantedPosition () // Send packet with player transforms
	{
		List<byte> packet = new List<byte>();
        
        packet.AddRange(BitConverter.GetBytes((ushort)0)); // Packet type
        packet.AddRange(BitConverter.GetBytes((ushort)game.playerID)); // Player id
        packet.AddRange(BitConverter.GetBytes((double)game.position.x)); // Player pos x
        packet.AddRange(BitConverter.GetBytes((double)game.position.z)); // Player pos z

        // Send packet
        socket.socket.Send(packet.ToArray(), SocketFlags.None);
	}
}