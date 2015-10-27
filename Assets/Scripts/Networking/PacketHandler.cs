using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using System.Net.Sockets;

public class PacketHandler : MonoBehaviour
{

    public Chat chat;
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
                ReceiveTextMessage(packet);
                break;
            case 5:
                ReceiveStatsUpdate(packet);
                break;
            case 6:
                ReceiveObjectState(packet);
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

        // Activate game UI & deactivate main menu
        game.gameUICamera.SetActive(true);
        game.mainMenuCamera.SetActive(false);

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
        chat.AddMessage(null, message + "! " + amountOfPlayers + " player(s) are connected.", true);
	}

    void ReceivePlayerConnected (byte[] packet)
    {
        // Convert data
        int playerID = BitConverter.ToInt16(packet, 2); // Player id
        float playerPosX = (float)BitConverter.ToDouble(packet, 4); // Player x pos
        float playerPosZ = (float)BitConverter.ToDouble(packet, 12); // Player z pos
        int playerNameLength = BitConverter.ToInt16(packet, 20); // Player name length
        string playerName = Encoding.ASCII.GetString(packet, 22, playerNameLength); // Player name

        // Create new NetworkPlayer
        if (playerID != game.playerID)
        {
            game.CreateNetworkPlayer(networkPlayer, playerID, playerPosX, playerPosZ, playerPosX, playerPosZ);
            chat.AddMessage(string.Empty, "Player with the name " + playerName + " has connected", true);
        }
    }

    void ReceivePlayerDisconnected (byte[] packet) 
    {
        // Convert data
        int playerID = BitConverter.ToUInt16(packet, 2); // Player id

        // Remove NetworkPlayer with ID
        game.RemoveNetworkPlayer(playerID);
        Debug.Log("Player with ID: " + playerID + " disconnected.");
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

    void ReceiveTextMessage (byte[] packet)
    {
        // Convert data store in variables
        bool gameMessage = BitConverter.ToBoolean(packet, 2);
        int playerNameLength = BitConverter.ToInt16(packet, 3);
        int playerMessageLength = BitConverter.ToInt16(packet, 5);
        string playerName = Encoding.ASCII.GetString(packet, 7, playerNameLength);
        string playerMessage = Encoding.ASCII.GetString(packet, 7 + playerNameLength, playerMessageLength);

        // Add message to chat
        chat.AddMessage(playerName, playerMessage, gameMessage);
    }

    void ReceiveStatsUpdate (byte[] packet)
    {
        // Convert data
        int attack = BitConverter.ToInt16(packet, 2); // Attack level
        int strength = BitConverter.ToInt16(packet, 4); // Strength level
        int defence = BitConverter.ToInt16(packet, 6); // Defence level
        int woodcutting = BitConverter.ToInt16(packet, 8); // Woodcutting level
        int attackXP = BitConverter.ToInt32(packet, 10); // Attack xp
        int strengthXP = BitConverter.ToInt32(packet, 14); // Strength xp
        int defenceXP = BitConverter.ToInt32(packet, 18); // Defence xp
        int woodcuttingXP = BitConverter.ToInt32(packet, 22); // Woodcutting xp
        int attackNeededXP = BitConverter.ToInt32(packet, 26); // Attack needed xp
        int strengthNeededXP = BitConverter.ToInt32(packet, 30); // Strength needed xp
        int defenceNeededXP = BitConverter.ToInt32(packet, 34); // Defence needed xp
        int woodcuttingNeededXP = BitConverter.ToInt32(packet, 38); // Woodcutting neededs xp

        // Check for level ups
        bool[] justLeveled = new bool[4];
        if (attack > game.attack && game.attack > 0)
        {
            chat.AddMessage(null, "You leveled up in the skill attack, new level is " + attack + ". Congratulations!", true);
            justLeveled[0] = true;
        }
        if (strength > game.strength && game.strength > 0)
        {
            chat.AddMessage(null, "You leveled up in the skill strength, new level is " + strength + ". Congratulations!", true);
            justLeveled[1] = true;
        }
        if (defence > game.defence && game.defence > 0)
        {
            chat.AddMessage(null, "You leveled up in the skill defence, new level is " + defence + ". Congratulations!", true);
            justLeveled[2] = true;
        }
        if (woodcutting > game.woodcutting && game.woodcutting > 0)
        {
            chat.AddMessage(null, "You leveled up in the skill woodcutting, new level is " + woodcutting + ". Congratulations!", true);
            justLeveled[3] = true;
        }

        game.attack = attack;
        game.strength = strength;
        game.defence = defence;
        game.woodcutting = woodcutting;


        // Insert in UI
        try
        {
            // Add level
            GameObject.Find("Stats_Attack").transform.FindChild("CurrentLevel").GetComponent<Text>().text = "Current level: " + attack.ToString();
            GameObject.Find("Stats_Strength").transform.FindChild("CurrentLevel").GetComponent<Text>().text = "Current level: " + strength.ToString();
            GameObject.Find("Stats_Defence").transform.FindChild("CurrentLevel").GetComponent<Text>().text = "Current level: " + defence.ToString();
            GameObject.Find("Stats_Woodcutting").transform.FindChild("CurrentLevel").GetComponent<Text>().text = "Current level: " + woodcutting.ToString();            
        }
        catch { }
    }

    void ReceiveObjectState (byte[] packet)
    {
        // Convert data
        int type = BitConverter.ToInt16(packet, 2); // Object type
        int id = BitConverter.ToInt16(packet, 4); // Object id
        bool state = BitConverter.ToBoolean(packet, 6); // Object state

        // Switch to find right object
        switch (type)
        {
            case 0:
                {
                    TreeData tree = game.GetTreeFromID(id);
                    tree.SetState(state);
                }
                break;
        }
    }

}