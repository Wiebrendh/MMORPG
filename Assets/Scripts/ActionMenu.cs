using UnityEngine;
using System.Collections;


public enum Actions { Null, Map, Player };
public enum MapActions { WalkHere, Examine };
public enum PlayerActions { WalkHere, RandomAction, Examine };

public class ActionMenu : MonoBehaviour
{

    public bool actionMenuActive;
    public Vector3 actionMenuWorldPos;
    public Vector2 actionMenuScreenPos;
    public Actions currentAction;
	
	void Update ()
    {
	    if (Input.GetButtonDown("Fire1") && !actionMenuActive) // Open action menu
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1024))
            {
                actionMenuActive = true;
                actionMenuWorldPos = Vector3.zero;
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
                            actionMenuActive = false;
                            actionMenuWorldPos = Vector3.zero;
                            actionMenuScreenPos = Vector2.zero;
                            currentAction = Actions.Null;
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
            GUI.Box(new Rect(actionMenuScreenPos, new Vector2(64, 40)), "Actions");
        }
    }

    private Vector2 MouseScreenPosToGUIPos () // Convert mouse position to GUI position
    {
        return new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
    }
}
