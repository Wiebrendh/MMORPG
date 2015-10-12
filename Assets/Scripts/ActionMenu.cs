using UnityEngine;
using System.Collections;
using System;


public enum Actions { Null, Map, Player, Enemy, Tree };
public enum MapActions { WalkHere, Examine };
public enum PlayerActions { WalkHere, RandomAction, Examine };
public enum EnemyActions { WalkHere, Attack, Examine };
public enum TreeActions { ChopDown, Examine };

public class ActionMenu : MonoBehaviour
{

    private Game game;
    private PacketSender sender;
    public LocalPlayer localPlayer;

    [Space(10)]

    public bool actionMenuActive;
    public Vector3 actionMenuWorldPos;
    public Vector2 actionMenuScreenPos;
    public GameObject actionMenuObject;
    public Actions currentAction;
	
    void Start ()
    {
        game = GameObject.Find("Networking").GetComponent<Game>();
        sender = GameObject.Find("Networking").GetComponent<PacketSender>();
    }

	void Update ()
    {
	    if (Input.GetButtonDown("Fire2") && !actionMenuActive) // Open action menu
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1024))
            {
                actionMenuActive = true;
                actionMenuWorldPos = hit.point;
                actionMenuScreenPos = MouseScreenPosToGUIPos();
                actionMenuObject = hit.transform.gameObject;

                string hitTag = hit.transform.tag;
                switch (hitTag)
                {
                    case "Terrain":
                        {
                            currentAction = Actions.Map;
                        }
                        break;
                    case "Player":
                        {
                            currentAction = Actions.Player;
                        }
                        break;
                    case "Tree":
                        {
                            currentAction = Actions.Tree;
                        }
                        break;
                    default:
                        {
                            CloseActionMenu();
                        }
                        break;
                }
            }
        }

        // Close action menu when open and Fire2 is up
        if (actionMenuActive && !Input.GetButton("Fire2"))
            CloseActionMenu();
	}

    void OnGUI () // GUI
    {
        if (actionMenuActive)
        {
            if (currentAction == Actions.Map)
                ActionMenu_Map();
            else if (currentAction == Actions.Tree)
                ActionMenu_Tree();
        }
    }

    private void ActionMenu_Map() // GUI action menu for the map
    {
        string[] names = Enum.GetNames(typeof(MapActions));

        GUI.Box(new Rect(actionMenuScreenPos, new Vector2(64, 82)), "Actions");

        for (int i = 0; i < 2; i++)
        {
            Rect rect = new Rect(actionMenuScreenPos.x + 7, actionMenuScreenPos.y + 25 + (25 * i), 50, 23);
            GUI.Box(rect, names[i]);

            if (rect.Contains(MouseScreenPosToGUIPos()) && Input.GetButtonUp("Fire2"))
            {
                switch (i)
                {
                    case 0:
                        {
                            Vector2 pos = ConvertToWalkPos(new Vector2(actionMenuWorldPos.x, actionMenuWorldPos.z));
                            sender.SendWantedPosition(new Vector2(pos.x, pos.y));
                            break;
                        }
                    case 1:
                        {
                            Debug.Log("The terrain.");
                            break;
                        }
                }
            }
        }

        if (Input.GetButtonUp("Fire2"))
            CloseActionMenu();
    }

    private void ActionMenu_Tree() // GUI action menu for a tree
    {
        string[] names = Enum.GetNames(typeof(TreeActions));

        GUI.Box(new Rect(actionMenuScreenPos, new Vector2(64, 82)), "Actions");

        for (int i = 0; i < 2; i++)
        {
            Rect rect = new Rect(actionMenuScreenPos.x + 7, actionMenuScreenPos.y + 25 + (25 * i), 50, 23);
            GUI.Box(rect, names[i]);

            if (rect.Contains(MouseScreenPosToGUIPos()) && Input.GetButtonUp("Fire2"))
            {
                switch (i)
                {
                    case 0:
                        {
                            TreeData tree = actionMenuObject.GetComponent<TreeData>();

                            // If player is able to chop tree
                            if (!tree.beingChopped && !tree.treeDown)
                            {
                                // Check if player is standing next to tree
                                if (Math.Round(Vector2.Distance(actionMenuObject.transform.position, localPlayer.transform.position), 1) <= 1)
                                    sender.SendTreeState(tree.treeID, 1);
                                else
                                    Debug.Log("You have to stand next to tree to chop it down.");
                            }
                            else Debug.Log("You cannot chop this tree (Already chopped, or someone else is chopping).");
                        }
                        break;
                    case 1:
                        {
                            Debug.Log("A tree you can chop down.");
                        }
                        break;
                }
            }
        }

        if (Input.GetButtonUp("Fire2"))
            CloseActionMenu();
    }

    private void CloseActionMenu () // Close the action menu
    {
        actionMenuActive = false;
        actionMenuWorldPos = Vector3.zero;
        actionMenuScreenPos = Vector2.zero;
        currentAction = Actions.Null;
    } 

    public static Vector2 MouseScreenPosToGUIPos () // Convert mouse position to GUI position
    {
        return new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
    }

    public static Vector2 ConvertToWalkPos (Vector2 value)
    { 
        Vector2 pos = new Vector2();

        if (Mathf.RoundToInt(value.x) > value.x)
            pos.x = Mathf.RoundToInt(value.x) - .5f;
        else
            pos.x = Mathf.RoundToInt(value.x) + .5f;

        if (Mathf.RoundToInt(value.y) > value.y)
            pos.y = Mathf.RoundToInt(value.y) - .5f;
        else
            pos.y = Mathf.RoundToInt(value.y) + .5f;

        return pos;
    }
}
