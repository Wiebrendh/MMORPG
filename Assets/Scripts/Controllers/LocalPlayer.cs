using UnityEngine;
using System.Collections;

public class LocalPlayer : MonoBehaviour
{

    private Game game;
    public NavMeshAgent agent;

    public Vector2 currentPos;
	
    void Start ()
    {
        game = GameObject.Find("Networking").GetComponent<Game>();
    }

	void Update ()
    {
        agent.updateRotation = false;
        agent.destination = game.playerPosition;

        currentPos = ActionMenu.ConvertToWalkPos(new Vector2(this.transform.position.x, this.transform.position.z));

	}
}
