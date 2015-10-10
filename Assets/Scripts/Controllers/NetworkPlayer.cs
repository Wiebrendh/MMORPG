using UnityEngine;
using System.Collections;

public class NetworkPlayer : MonoBehaviour 
{

    public NavMeshAgent agent;

    public int playerID;
    public Vector3 playerPosition;

    void Update ()
    {
        agent.destination = playerPosition;
    }

}
