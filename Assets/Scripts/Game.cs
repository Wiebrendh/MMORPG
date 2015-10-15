using UnityEngine;
using System.Collections.Generic;

public class Game : MonoBehaviour 
{

    public PacketSender sender;

    public List<NetworkPlayer> networkPlayers = new List<NetworkPlayer>();
    public TreeData[] treeDatas;

    // Player data
    public int playerID;
    public string playerName;
    public Vector3 playerPosition;

    void Start ()
    {
        treeDatas = GameObject.FindObjectsOfType<TreeData>();
    }

    public void CreateNetworkPlayer (GameObject prefab, int id, float posX, float posZ, float posCurrX, float posCurrZ)
    {
        GameObject obj = Instantiate(prefab, new Vector3(posCurrX, 1, posCurrZ), Quaternion.identity) as GameObject;
        NetworkPlayer player = obj.GetComponent<NetworkPlayer>();
        player.playerPosition = new Vector3(posX, 1, posZ);
        player.playerID = id;
        networkPlayers.Add(player);
    }

    public void RemoveNetworkPlayer (int id)
    {
        foreach (NetworkPlayer player in networkPlayers)
        {
            if (player.playerID == id)
            {
                player.disconnected = true;
            }
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

    public TreeData GetTreeFromID (int id)
    {
        foreach (TreeData tree in treeDatas)
        {
            if (tree.treeID == id)
                return tree;
        }
        return null;
    }
}
