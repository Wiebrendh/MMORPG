using UnityEngine;
using System.Collections.Generic;

public class Game : MonoBehaviour 
{

    public PacketSender sender;

    // Game data
    public bool canSpawnPlayer;
    public GameObject localPlayerPrefab;

    public List<NetworkPlayer> networkPlayers = new List<NetworkPlayer>();

    // Player data
    public int playerID;
    public string playerName;
    public Vector3 playerPosition;
	
	void Start () 
    {
	    
	}
	
	void Update () 
    {
        // Can spawn player
        if (canSpawnPlayer)
        {
            Instantiate(localPlayerPrefab, new Vector3(playerPosition.x, 1, playerPosition.z), Quaternion.identity).name = "LocalPlayer";
            canSpawnPlayer = false;
        }
	}

    public NetworkPlayer GetNetworkPlayerFromID (int id)
    {
        foreach (NetworkPlayer player in networkPlayers)
        {
            if (player.playerID == id)
                return player;
        }
        return null;
    }
}
