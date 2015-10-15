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
        }
        else if (Time.time < currentSendTime)
            AddMessage(textField.text);
            
	}

    public void AddMessage (string message)
    {
        Debug.Log(message);
    }
}
