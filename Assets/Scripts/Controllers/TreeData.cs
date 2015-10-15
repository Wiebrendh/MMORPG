using UnityEngine;
using System.Collections;

public class TreeData : MonoBehaviour 
{

    public Game game;
    public PacketSender sender;
    private LocalPlayer player;

    public GameObject treeUp;
    public GameObject treeStump;

    public int treeID;
    public bool beingChopped;
    public bool treeDown;

    public float secondTimer;
    public int choppingTimeLeft;
    public int reupTime;

    void Start ()
    {
        this.transform.FindChild("Graphical").eulerAngles = new Vector3(0, Random.Range(0, 360), 0);
    }

    void Update ()
    {
        if (player == null)
        {
            GameObject obj = GameObject.Find("LocalPlayer");
            if (obj)
                player = obj.GetComponent<LocalPlayer>();
        }
        else
        {
            if (Time.time > secondTimer) // Timer runs every second
            {
                secondTimer = Time.time + 1;

                if (choppingTimeLeft > 0) // If player is chopping tree
                {
                    choppingTimeLeft--;

                    if (choppingTimeLeft == 0) // If player is done chopping tree
                    {
                        reupTime = 20;
                        sender.SendTreeState(treeID, 2);
                        player.canDoAction = true;
                    }
                }

                if (reupTime > 0) // If tree is down
                {
                    reupTime--;

                    if (reupTime == 0) // If tree is reupped
                    {
                        sender.SendTreeState(treeID, 0);
                    }
                }
            }
        }
    }

    public void SetState (int state, int choppedByID)
    {
        if (state == 0)
        {
            treeDown = false;
            beingChopped = false;
            treeUp.SetActive(true);
            treeStump.SetActive(false);
        }
        if (state == 1)
        {
            treeDown = false;
            beingChopped = true;
            
            if (choppedByID == game.playerID)
            {
                choppingTimeLeft = 20;
            }
        }
        if (state == 2) 
        {
            treeDown = true;
            beingChopped = false;
            treeUp.SetActive(false);
            treeStump.SetActive(true);
        }
    }

    void OnApplicationQuit ()
    {
        if (choppingTimeLeft > 0 || reupTime > 0)
            sender.SendTreeState(treeID, 0);
    }
}
