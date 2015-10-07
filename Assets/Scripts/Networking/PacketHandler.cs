using UnityEngine;
using System;
using System.Text;
using System.Net.Sockets;

public class PacketHandler : MonoBehaviour
{

    public Game game;
    public PacketSender sender;

    public void Handle(byte[] packet, Socket _socket) // Get the packet, and transfer it to its private void
	{
		ushort packetType = BitConverter.ToUInt16(packet, 0);
		
		switch (packetType) // Call the right function
		{
		    case 0:
			    ConnectedToServer(packet, _socket);
			    break;
		}
	}
	
	void ConnectedToServer(byte[] packet, Socket _socket) // When the player connected to the server
	{
        // Convert data
        game.playerID = (int)BitConverter.ToUInt16(packet, 2); // Player ID
        int messageLength = BitConverter.ToUInt16(packet, 4); // Message length
		string message = Encoding.ASCII.GetString(packet, 6, messageLength); //  Message
        bool newPlayer = BitConverter.ToBoolean(packet, 6 + messageLength); // Bool newPlayer

        // Get player position from server
        Vector3 spawnPos = Vector3.zero; 
        if (!newPlayer)
        {
            // Convert data
            float posX = (float)BitConverter.ToDouble(packet, 7 + messageLength); // Get xPos
            float posY = (float)BitConverter.ToDouble(packet, 15 + messageLength); // Get yPos
            float posZ = (float)BitConverter.ToDouble(packet, 23 + messageLength); // Get zPos
            spawnPos = new Vector3(posX, posY, posZ); // Convert to vector3
        }

        // Debug message
        print("Message from server: " + message);
	}
}