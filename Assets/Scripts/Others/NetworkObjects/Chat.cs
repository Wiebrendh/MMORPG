using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Chat : MonoBehaviour
{

    public PacketSender sender;
    public InputField textField;

    public float maxSendRate;
    private float currentSendTime;

	void Update ()
    {
        if (textField.text.Length > 0 && Time.time >= currentSendTime && Input.GetKeyDown(KeyCode.Return))
        {
            currentSendTime = Time.time + maxSendRate;
            sender.SendTextMessage(textField.text);
            textField.text = string.Empty;
        }            
	}

    public void AddMessage (string message)
    {
        Debug.Log(message);
    }
}
