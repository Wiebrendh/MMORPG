﻿using System;
using UnityEngine;
using UnityEngine.EventSystems;


public enum Actions { Null, Map, Player, Enemy, Tree };
public enum MapActions { WalkHere, Examine };
public enum PlayerActions { WalkHere, Follow, Examine };
public enum EnemyActions { WalkHere, Attack, Examine };
public enum TreeActions { ChopDown, Examine };

public class ActionMenu : MonoBehaviour
{

    private Game game;
    private Chat chat;
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
        chat = GameObject.Find("Chat").GetComponent<Chat>();
    }

	void Update ()
    {
        #region Action menu
        // Action menu
        if (Input.GetButtonDown("Fire2") && !actionMenuActive && localPlayer.canDoAction && !EventSystem.current.IsPointerOverGameObject()) // Open action menu
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
        #endregion

        #region Quick action
        // Quick action
        if (Input.GetButtonDown("Fire1") && !actionMenuActive && localPlayer.canDoAction && !EventSystem.current.IsPointerOverGameObject()) // Perfrom quick action
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1024))
            {
                actionMenuWorldPos = hit.point;
                actionMenuScreenPos = MouseScreenPosToGUIPos();
                actionMenuObject = hit.transform.gameObject;

                string hitTag = hit.transform.tag;
                switch (hitTag)
                {
                    case "Terrain": // Quick action for map
                        {
                            Vector2 pos = ConvertToWalkPos(new Vector2(actionMenuWorldPos.x, actionMenuWorldPos.z));
                            sender.SendWantedPosition(new Vector2(pos.x, pos.y));
                        }
                        break;
                    case "Player": // Quick action for tree
                        {
                            Vector2 pos = ConvertToWalkPos(new Vector2(actionMenuWorldPos.x, actionMenuWorldPos.z));
                            sender.SendWantedPosition(new Vector2(pos.x, pos.y));
                        }
                        break;
                    case "Tree":
                        {
                            TreeData tree = actionMenuObject.GetComponent<TreeData>();

                            // If player is able to chop tree
                            if (!tree.beingChopped && !tree.treeDown)
                            {
                                // Check if player is standing next to tree
                                if (Math.Round(Vector3.Distance(actionMenuObject.transform.position, localPlayer.transform.position), 1) <= 1.8f)
                                {
                                    sender.SendTreeState(tree.treeID, 1);
                                    localPlayer.canDoAction = false;
                                }
                                else
                                    Debug.Log("You have to stand next to tree to chop it down.");
                            }
                            else Debug.Log("You cannot chop this tree (Already chopped, or someone else is chopping).");
                        }
                        break;
                }
            }
        }
        #endregion

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
            else if (currentAction == Actions.Player)
                ActionMenu_Player();
            else if (currentAction == Actions.Tree)
                ActionMenu_Tree();
        }
    }

    #region Action menus
    private void ActionMenu_Map() // GUI action menu for the map
    {
        string[] names = Enum.GetNames(typeof(MapActions));

        GUI.Box(new Rect(actionMenuScreenPos, new Vector2(64, 30 + (names.Length * 25))), "Actions");

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
                        }
                        break;
                    case 1:
                        {
                            chat.AddMessage(null, "The terrain.", true);
                        }
                        break;
                }
            }
        }

        if (Input.GetButtonUp("Fire2"))
            CloseActionMenu();
    }

    private void ActionMenu_Tree() // GUI action menu for a tree
    {
        string[] names = Enum.GetNames(typeof(TreeActions));

        GUI.Box(new Rect(actionMenuScreenPos, new Vector2(64, 30 + (names.Length * 25))), "Actions");

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
                                if (Math.Round(Vector3.Distance(actionMenuObject.transform.position, localPlayer.transform.position), 1) <= 1.8f)
                                {
                                    sender.SendTreeState(tree.treeID, 1);
                                    localPlayer.canDoAction = false;
                                }
                                else chat.AddMessage(null, "You have to stand next to tree to chop it down.", true);
                            }
                            else chat.AddMessage(null, "You cannot chop this tree, the tree is down or already being chopped.", true);
                        }
                        break;
                    case 1:
                        {
                            chat.AddMessage(null, "A tree you can chop down.", true);
                        }
                        break;
                }
            }
        }

        if (Input.GetButtonUp("Fire2"))
            CloseActionMenu();
    }

    private void ActionMenu_Player() // GUI action menu for a tree
    {
        string[] names = Enum.GetNames(typeof(PlayerActions));

        GUI.Box(new Rect(actionMenuScreenPos, new Vector2(64, 30 + (names.Length * 25))), "Actions");

        for (int i = 0; i < 3; i++)
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
                        }
                        break;
                    case 1:
                        {
                            // Calculate the best position
                            
                        }
                        break;
                    case 2:
                        {
                            int id = actionMenuObject.GetComponent<NetworkPlayer>().playerID;
                            chat.AddMessage(null, "A player called '" + id + "'.", true);
                        }
                        break;
                }
            }
        }

        if (Input.GetButtonUp("Fire2"))
            CloseActionMenu();
    }
    #endregion 

    #region Functions
    private void CloseActionMenu () // Close the action menu
    {
        actionMenuActive = false;
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
    #endregion
}