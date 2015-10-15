using UnityEngine;
using System;
using System.Text;
using System.Net.Sockets;

public class PacketHandler : MonoBehaviour
{

    public Game game;
    public PacketSender sender;

    public GameObject localPlayer;
    public GameObject networkPlayer;

    public void Handle(byte[] packet, Socket _socket) // Get the packet, and transfer it to its private void
	{
		ushort packetType = BitConverter.ToUInt16(packet, 0);
		
		switch (packetType) // Call the right function
		{
		    case 0: // When you connected to the server
			    ConnectedToServer(packet);
			    break;
            case 1:
                ReceivePlayerConnected(packet);
                break;
            case 2:
                ReceivePlayerDisconnected(packet);
                break;
            case 3:
                ReceivePlayerPosition(packet);
                break;
            case 4:
                ReceiveTreeState(packet);
                break;
		}
	}
	
	void ConnectedToServer (byte[] packet)
	{
        // Convert data
        game.playerID = BitConverter.ToUInt16(packet, 2); // Player ID
        int messageLength = BitConverter.ToUInt16(packet, 4); // Message length
		string message = Encoding.ASCII.GetString(packet, 6, messageLength); //  Message

        // Receive player position
        game.playerPosition.x = (float)BitConverter.ToDouble(packet, 6 + messageLength);
        game.playerPosition.z = (float)BitConverter.ToDouble(packet, 14 + messageLength);

        // Spawn player if everything is done
        Instantiate(localPlayer, new Vector3(game.playerPosition.x, 1, game.playerPosition.z), Quaternion.identity).name = "LocalPlayer";

        // Receive players on server, his current position and their wanted position
        int amountOfPlayers = BitConverter.ToUInt16(packet, 22 + messageLength);
        
        // Get player data
        int startBit = 24 + messageLength;
        for (int i = 0; i < amountOfPlayers; i++)
        {
            float playerID = 0, currX = 0, currZ = 0, wantedX = 0, wantedZ = 0;
            playerID = BitConverter.ToUInt16(packet, startBit);
            currX = (float)BitConverter.ToDouble(packet, startBit + 2);
            currZ = (float)BitConverter.ToDouble(packet, startBit + 10);
            wantedX = (float)BitConverter.ToDouble(packet, startBit + 18);
            wantedZ = (float)BitConverter.ToDouble(packet, startBit + 26);
            startBit += 34;

            // Use data to spawn network client
            if (playerID != game.playerID)
                game.CreateNetworkPlayer(networkPlayer, (int)playerID, wantedX, wantedZ, currX, currZ);
        }

        // Debug message
        print("Message from server: " + message + ". " + amountOfPlayers + " player(s) are connected.");
	}

    void ReceivePlayerConnected (byte[] packet)
    {
        // Convert data
        int playerID = BitConverter.ToUInt16(packet, 2); // Player id
        float playerPosX = (float)BitConverter.ToDouble(packet, 4); // Player x pos
        float playerPosZ = (float)BitConverter.ToDouble(packet, 12); // Player z pos

        // Create new NetworkPlayer
        if (playerID != game.playerID)
        {
            game.CreateNetworkPlayer(networkPlayer, playerID, playerPosX, playerPosZ, playerPosX, playerPosZ);
            Debug.Log("Player with ID: " + playerID + " connected.");
        }
    }

    void ReceivePlayerDisconnected (byte[] packet) 
    {
        // Convert data
        int playerID = BitConverter.ToUInt16(packet, 2); // Player id

        // Remove NetworkPlayer with ID
        game.RemoveNetworkPlayer(playerID);
        Debug.Log("Player with ID: " + playerID + " connected.");
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
            Debug.Log("Player with ID: " + playerID + " walked towards: " + playerPosX + "|" + playerPosZ);

        }
        else if (playerID == game.playerID) game.playerPosition = new Vector3(playerPosX, 1, playerPosZ);
    }

    void ReceiveTreeState (byte[] packet)
    {
        // Convert data
        int treeID = BitConverter.ToInt16(packet, 2); // Tree id
        int treeState = BitConverter.ToInt16(packet, 4); // Get tree state
        int choppedByID = BitConverter.ToInt16(packet, 6); // Get chopped by id

        // Insert to the correct TreeData
        TreeData tree = game.GetTreeFromID(treeID);
        if (tree != null)
        {
            tree.SetState(treeState, choppedByID);
        }
    }
}