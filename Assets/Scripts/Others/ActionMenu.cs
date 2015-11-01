using System;
using UnityEngine;
using UnityEngine.EventSystems;


public enum Actions { Null, Map, Player, Enemy, Tree };

public class ActionMenu : MonoBehaviour
{

    private Game game;
    private Chat chat;
    private SideObjects side;
    private PacketSender sender;
    public LocalPlayer localPlayer;

    [Space(10)]

    private Vector2 touchStartPos;

    public bool isMenuOpen;
    public Vector2 touchPos;
    public Actions currentAction;

    void Start()
    {
        game = GameObject.Find("Networking").GetComponent<Game>();
        sender = GameObject.Find("Networking").GetComponent<PacketSender>();
        chat = GameObject.Find("Chat").GetComponent<Chat>();
        side = GameObject.Find("SideObjects").GetComponent<SideObjects>();
    }

    void Update()
    {
        // Get the new touch
        if (Input.touchCount == 1)
        {
            // Store touch in variable
            Touch touch = Input.GetTouch(0);

            // Check if touch just started
            if (touch.phase == TouchPhase.Began && side.currentType == SideObjects.Types.Null)
                touchStartPos = touch.position;

            // Check if the touch ended
            if (touch.phase == TouchPhase.Ended && touchStartPos != Vector2.zero && side.currentType == SideObjects.Types.Null)
            {
                // Check when the touch ended, if the finger is still close to start point
                Vector2 difference = touchStartPos - touch.position;
                if (Math.Abs(difference.x) < 5 && Math.Abs(difference.y) < 5)
                {
                    isMenuOpen = true;
                    touchPos = touch.position;

                    // Raycast on position
                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(touchPos);
                    if (Physics.Raycast(ray, out hit) && !EventSystem.current.IsPointerOverGameObject(-1))
                    {
                        side.currentType = SideObjects.Types.ActionMenu;

                        // Set currentAction
                        switch (hit.transform.tag)
                        {
                            case "Terrain":
                                currentAction = Actions.Map;
                                break;
                            case "Player":
                                currentAction = Actions.Player;
                                break;
                            case "Enemy":
                                currentAction = Actions.Enemy;
                                break;
                            case "Tree":
                                currentAction = Actions.Tree;
                                break;
                        }
                    }
                }
                touchStartPos = Vector2.zero;
            }
        }
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
}
