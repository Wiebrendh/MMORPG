using System;
using UnityEngine;

public class SideObjects : MonoBehaviour
{

    private ActionMenu actionMenu;

    public enum Types { Null = -1, ActionMenu = 0, Chat = 1 };
    public Types currentType;
    public GameObject[] sideObjects;
    public GameObject[] actionMenuItems;    
    
	void Start ()
    {
        currentType = Types.Null;
	}
	
	void Update ()
    {
        // Make sure you have the ActionMenu component
        if (actionMenu == null && GameObject.Find("Camera"))
            actionMenu = GameObject.Find("Camera").GetComponent<ActionMenu>();

        //  Return if action menu is not set
        if (actionMenu == null)
            return;

        // Move objects to correct position
        foreach (Types type in Enum.GetValues(typeof(Types)))
        {
            // Decide to make visible or invisible
            if (type != Types.Null && type == currentType)
            {
                sideObjects[(int)type].SetActive(true);
            }
            else if (type != Types.Null && type != currentType)
            {
                sideObjects[(int)type].SetActive(false);
            }

            // Check if this is the action menu. If yes, loop through his child objects
            if (type == Types.ActionMenu)
            {
                foreach (GameObject obj in actionMenuItems)
                {
                    // Enable/disable children
                    if (obj.name == actionMenu.currentAction.ToString())
                    {
                        for (int amount = 0; amount < obj.transform.childCount; amount++)
                        {
                            obj.transform.GetChild(amount).gameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        for (int amount = 0; amount < obj.transform.childCount; amount++)
                        {
                            obj.transform.GetChild(amount).gameObject.SetActive(false);
                        }
                    }
                }
            }
        }

        // Loop through touches
        int i = 0;
        while (i < Input.touchCount)
        {
            // Check if input is to the right of the UI
            if (Input.GetTouch(i).position.x > 300 && Input.GetTouch(i).phase == TouchPhase.Ended)
                currentType = Types.Null;

            i++;
        }
    }

    public void ChangeCurrent (int type)
    {
        
    }
}
