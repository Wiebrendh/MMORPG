using UnityEngine;
using System.Collections;

public class NetworkPlayer : MonoBehaviour 
{

    public Game game;
    public NavMeshAgent agent;

    public bool disconnected;

    public int playerID;
    public Vector3 playerPosition;

    void Start ()
    {
        game = GameObject.Find("Networking").GetComponent<Game>();
    }

    void Update ()
    {
        agent.destination = playerPosition;

        if (disconnected)
        {
            game.networkPlayers.Remove(this.GetComponent<NetworkPlayer>());
            Destroy(this.gameObject);
        }
    }
}
