using System;
using UnityEngine;
using UnityEngine.EventSystems;


public enum Actions_Old { Null, Map, Player, Enemy, Tree };
public enum MapActions_Old { WalkHere, Examine };
public enum PlayerActions_Old { WalkHere, Follow, Examine };
public enum EnemyActions_Old { WalkHere, Attack, Examine };
public enum TreeActions_Old { ChopDown, Examine };

public class ActionMenu_Old : MonoBehaviour
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
    public Actions_Old currentAction;

    void Start()
    {
        game = GameObject.Find("Networking").GetComponent<Game>();
        sender = GameObject.Find("Networking").GetComponent<PacketSender>();
        chat = GameObject.Find("Chat").GetComponent<Chat>();
    }

    void Update()
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
                            currentAction = Actions_Old.Map;
                        }
                        break;
                    case "Player":
                        {
                            currentAction = Actions_Old.Player;
                        }
                        break;
                    case "Tree":
                        {
                            currentAction = Actions_Old.Tree;
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

                            // Check if player is standing next to tree
                            if (Math.Round(Vector3.Distance(actionMenuObject.transform.position, localPlayer.transform.position), 1) <= 1.8f)
                            {
                                sender.SendHarvestObject(0, tree.treeID);
                            }
                            else
                                Debug.Log("You have to stand next to tree to chop it down.");
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

    void OnGUI() // OnGUI
    {
        if (actionMenuActive)
        {
            if (currentAction == Actions_Old.Map)
                ActionMenu_Map();
            else if (currentAction == Actions_Old.Player)
                ActionMenu_Player();
            else if (currentAction == Actions_Old.Tree)
                ActionMenu_Tree();
        }
    }

    #region Action menus
    private void ActionMenu_Map() // GUI action menu for the map
    {
        string[] names = Enum.GetNames(typeof(MapActions_Old));

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
                            if (actionMenuWorldPos.y == 0.00714159f)
                            {
                                Vector2 pos = ConvertToWalkPos(new Vector2(actionMenuWorldPos.x, actionMenuWorldPos.z));
                                sender.SendWantedPosition(new Vector2(pos.x, pos.y));
                            }
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
        string[] names = Enum.GetNames(typeof(TreeActions_Old));

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

                            // Check if player is standing next to tree
                            if (Math.Round(Vector3.Distance(actionMenuObject.transform.position, localPlayer.transform.position), 1) <= 1.8f)
                            {
                                sender.SendHarvestObject(0, tree.treeID);
                            }
                            else chat.AddMessage(null, "You have to stand next to tree to chop it down.", true);
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
        string[] names = Enum.GetNames(typeof(PlayerActions_Old));

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
    private void CloseActionMenu() // Close the action menu
    {
        actionMenuActive = false;
    }

    public static Vector2 MouseScreenPosToGUIPos() // Convert mouse position to GUI position
    {
        return new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
    }

    public static Vector2 ConvertToWalkPos(Vector2 value)
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
