using UnityEngine;
using System.Collections;

public class LocalPlayer : MonoBehaviour
{

    public Game game;
    public NavMeshAgent agent;
	
	void Update ()
    {
        agent.updateRotation = false;
        agent.destination = game.position;
	}
}
