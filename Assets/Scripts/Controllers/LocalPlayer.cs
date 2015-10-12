using UnityEngine;
using System.Collections;

public class LocalPlayer : MonoBehaviour
{

    private Game game;
    public NavMeshAgent agent;
    public PacketSender sender;

    public Vector2 currentPos;
    private float posUpdateTime;
    	
    void Start ()
    {
        game = GameObject.Find("Networking").GetComponent<Game>();
        sender = GameObject.Find("Networking").GetComponent<PacketSender>();
    }

	void Update ()
    {
        agent.updateRotation = false;
        agent.destination = game.playerPosition;

        currentPos = ActionMenu.ConvertToWalkPos(new Vector2(this.transform.position.x, this.transform.position.z));

        if (agent.velocity != Vector3.zero && Time.time > posUpdateTime)
        {
            posUpdateTime = Time.time + .1f;
            sender.SendCurrentPos(new Vector2(this.transform.position.x, this.transform.position.z));
        }
	}
}
