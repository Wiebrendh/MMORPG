using UnityEngine;
using System.Collections;

public class TreeData : MonoBehaviour 
{

    public GameObject treeUp;
    public GameObject treeStump;

    public int treeID;

    void Start ()
    {
        int randomRotation = Random.Range(0, 360);
        this.transform.FindChild("Graphical_TreeUp").eulerAngles = new Vector3(0, randomRotation, 0);
        this.transform.FindChild("Graphical_TreeStump").eulerAngles = new Vector3(0, randomRotation, 0);
    }

    void Update ()
    {
        
    }

    public void SetState (bool state)
    {
        if (state) // If tree is up
        {
            treeUp.SetActive(true);
            treeStump.SetActive(false);
        }
        else // If tree is down
        {
            treeUp.SetActive(false);
            treeStump.SetActive(true);
        }
    }
}
