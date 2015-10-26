using System;
using UnityEngine;
using System.Collections.Generic;

public enum SelectedItem { JustJoined = 0, Levels = 1, Inventory = 2, Equipment = 3, Exit = 4 };

public class Toolbar : MonoBehaviour
{

    public PacketSender sender;

    public SelectedItem item;
    public List<GameObject> itemObjects = new List<GameObject>();

    public void SetSelectedItem (string itemName)
    {
        // Deactivate the old selected item
        itemObjects[(int)item].SetActive(false);

        // Set item to new selected item
        item = (SelectedItem)Enum.Parse(typeof(SelectedItem), itemName);

        // Activate the new selected item
        itemObjects[(int)item].SetActive(true);

        // Check if client has to request data
        if (itemName == "Levels")
            sender.SendLevelsRequest();
    }
}
