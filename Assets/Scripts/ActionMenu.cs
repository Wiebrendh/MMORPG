using UnityEngine;
using System.Collections;
using System;


public enum Actions { Null, Map, Player };
public enum MapActions { WalkHere, Examine };
public enum PlayerActions { WalkHere, RandomAction, Examine };

public class ActionMenu : MonoBehaviour
{

    public PacketSender sender;

    [Space(10)]

    public bool actionMenuActive;
    public Vector3 actionMenuWorldPos;
    public Vector2 actionMenuScreenPos;
    public Actions currentAction;
	
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
                    default:
                        {
                            CloseActionMenu();
                        }
                        break;
                }
            }
        }
	}

    void OnGUI ()
    {
        if (actionMenuActive)
        {
            if (currentAction == Actions.Map)
                ActionMenu_Map();
        }
    }

    private Vector2 MouseScreenPosToGUIPos () // Convert mouse position to GUI position
    {
        return new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
    }

    private void ActionMenu_Map ()
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
                            Vector2 pos = new Vector2(Mathf.RoundToInt(actionMenuWorldPos.x), Mathf.RoundToInt(actionMenuWorldPos.z));
                            sender.SendWantedPosition(pos);
                            CloseActionMenu();
                            break;
                        }
                    case 1:
                        {
                            Debug.Log("A location you can walk towards.");
                            CloseActionMenu();
                            break;
                        }                
                }
            }
        }
    }

    private void CloseActionMenu ()
    {
        actionMenuActive = false;
        actionMenuWorldPos = Vector3.zero;
        actionMenuScreenPos = Vector2.zero;
        currentAction = Actions.Null;
    }
}
