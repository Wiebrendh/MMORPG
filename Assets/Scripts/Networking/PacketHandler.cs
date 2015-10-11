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
			    ConnectedToServer(packet);
			    break;
            case 1:
                Debug.Log("1");
                break;
            case 2:

            case 3:
                ReceivePlayerPosition(packet);
                break;
		}
	}
	
	void ConnectedToServer(byte[] packet) // When the player connected to the server
	{
        // Convert data
        game.playerID = BitConverter.ToUInt16(packet, 2); // Player ID
        int messageLength = BitConverter.ToUInt16(packet, 4); // Message length
		string message = Encoding.ASCII.GetString(packet, 6, messageLength); //  Message

        // Receive player position
        game.playerPosition.x = (float)BitConverter.ToDouble(packet, 6 + messageLength);
        game.playerPosition.z = (float)BitConverter.ToDouble(packet, 14 + messageLength);

        // Spawn player if everything is done
        game.canSpawnPlayer = true;

        // Debug message
        print("Message from server: " + message);
	}

    void ReceivePlayerConnected (byte[] packet)
    {
        // Convert data
        int playerID = BitConverter.ToUInt16(packet, 2); // Player id
        float playerPosX = (float)BitConverter.ToDouble(packet, 4); // Player x pos
        float playerPosZ = (float)BitConverter.ToDouble(packet, 12); // Player z pos
    }

    void ReceivePlayerPosition (byte[] packet)
    {
        // Convert data
        int playerID = BitConverter.ToUInt16(packet, 2); // Player id
        float playerPosX = (float)BitConverter.ToDouble(packet, 4); // Player x pos
        float playerPosZ = (float)BitConverter.ToDouble(packet, 12); // Player z pos 

        // Insert to correct NetworkPlayer
        NetworkPlayer player = game.GetNetworkPlayerFromID(playerID);
        if (player != null)
        {
            player.playerPosition = new Vector3(playerPosX, 1, playerPosZ);
        }
        else game.playerPosition = new Vector3(playerPosX, 1, playerPosZ);
    }
}