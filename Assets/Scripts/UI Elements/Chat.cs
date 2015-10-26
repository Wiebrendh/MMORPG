using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Chat : MonoBehaviour
{

    public PacketSender sender;
    public InputField textField;

    public float maxSendRate;
    private float currentSendTime;

    public List<Text> messageBoxes = new List<Text>();

    void Update()
    {
        if (textField.text.Length > 0 && Time.time >= currentSendTime && Input.GetKeyDown(KeyCode.Return))
        {
            currentSendTime = Time.time + maxSendRate;
            sender.SendTextMessage(textField.text);
            textField.text = string.Empty;
        }
    }

    public void AddMessage (string sender, string message, bool gameNotification)
    {
        for (int i = 11; i > 0; i--)
        {
            if (i != 0)
                messageBoxes[i].text = messageBoxes[i - 1].text;
        }

        if (!gameNotification)
            messageBoxes[0].text = sender + ": " + "<color=#00ADFCFF>" + message + "</color>";
        else
            messageBoxes[0].text = "<color=#FFFFFFFF><b>" + message + "</b></color>";
    }
}
