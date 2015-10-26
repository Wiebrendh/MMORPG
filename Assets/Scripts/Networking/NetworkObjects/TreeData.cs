using UnityEngine;
using System.Collections;

public class TreeData : MonoBehaviour 
{

    public GameObject treeUp;
    public GameObject treeStump;

    public int treeID;
    public bool beingChopped;
    public bool treeDown;

    void Start ()
    {
        this.transform.FindChild("Graphical_TreeUp").eulerAngles = new Vector3(0, Random.Range(0, 360), 0);
    }

    void Update ()
    {
        
    }

    public void SetState (int state, int choppedByID)
    {
        if (state == 0)
        {
            treeDown = false;
            beingChopped = false;
            treeUp.SetActive(true);
            treeStump.SetActive(false);
        }
        if (state == 1)
        {
            treeDown = false;
            beingChopped = true;
        }
        if (state == 2) 
        {
            treeDown = true;
            beingChopped = false;
            treeUp.SetActive(false);
            treeStump.SetActive(true);
        }
    }
}
