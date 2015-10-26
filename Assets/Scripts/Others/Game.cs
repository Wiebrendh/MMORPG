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
    public int attack = 0, strength = 0, defence = 0, woodcutting = 0;

    // Connection items
    public GameObject mainMenuCamera;
    public GameObject gameUICamera;

    void Start ()
    {
        treeDatas = GameObject.FindObjectsOfType<TreeData>();
    }

    public void CreateNetworkPlayer (GameObject prefab, int id, float posX, float posZ, float posCurrX, float posCurrZ)
    {
        // Spawn player
        GameObject obj = Instantiate(prefab, new Vector3(posCurrX, 1, posCurrZ), Quaternion.identity) as GameObject;
        NetworkPlayer player = obj.GetComponent<NetworkPlayer>();
        player.playerPosition = new Vector3(posX, 1, posZ);
        player.playerID = id;
        networkPlayers.Add(player);
    }

    public void Logout ()
    {
        // Disable objects that are only wanted when connected
        gameUICamera.SetActive(false);

        // Remove every network object
        List<GameObject> objects = new List<GameObject>();
        objects.Add(GameObject.Find("LocalPlayer"));
        objects.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        objects.AddRange(GameObject.FindGameObjectsWithTag("Enemy"));

        foreach (GameObject obj in objects)
            Destroy(obj);

        // Send disconnect to server
        sender.Disconnect(playerID);

        // Enable main menu camera
        mainMenuCamera.SetActive(true);
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
