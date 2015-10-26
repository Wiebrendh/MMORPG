using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour 
{

    public int speed;
    public GameObject character;
	
	void Update () 
    {
        this.transform.LookAt(character.transform.position);

        // Horizontal (Keyboard left and right)
        this.transform.RotateAround(character.transform.position, new Vector3(0, Input.GetAxis("Horizontal"), 0), -Mathf.Abs(Input.GetAxis("Horizontal") * Time.deltaTime * speed));

        // Horizontal (Mouse middle mouse and drag)
        if (Input.GetButton("Fire3") && Input.GetAxis("Mouse X") != 0)
            this.transform.RotateAround(character.transform.position, new Vector3(0, Input.GetAxis("Mouse X"), 0), -Mathf.Abs(Input.GetAxis("Mouse X") * Time.deltaTime * -speed));
    }
}
