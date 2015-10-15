using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour 
{

    public int speed;
    public GameObject character;
	
	void Update () 
    {
        this.transform.LookAt(character.transform.position);

        /* Vertical
        if (this.transform.eulerAngles.x > 20 && this.transform.eulerAngles.x < 80)
            this.transform.RotateAround(character.transform.position, new Vector3(Input.GetAxis("Vertical"), 0, 0), Mathf.Abs(Input.GetAxis("Vertical") * Time.deltaTime * speed));

        float fixedX = Mathf.Clamp(this.transform.eulerAngles.x, 20, 80);
        this.transform.eulerAngles = new Vector3(fixedX, this.transform.eulerAngles.y, 0);*/

        // Vertical
        this.transform.RotateAround(character.transform.position, new Vector3(0, Input.GetAxis("Horizontal"), 0), -Mathf.Abs(Input.GetAxis("Horizontal") * Time.deltaTime * speed));
	}
}
