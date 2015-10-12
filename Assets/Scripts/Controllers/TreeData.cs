using UnityEngine;
using System.Collections;

public class TreeData : MonoBehaviour 
{

    public int treeID;
    public bool beingChopped;
    public bool treeDown;

    void Update ()
    {
        if (treeDown)
            this.GetComponent<Renderer>().material.color = Color.red;
        else if (beingChopped)
            this.GetComponent<Renderer>().material.color = Color.yellow;
        else
            this.GetComponent<Renderer>().material.color = Color.white;
    }

    public void TreeDowned ()
    {

    }

    public void SetState (int state)
    {
        if (state == 0)
        {
            treeDown = false;
            beingChopped = false;
        }
        else if (state == 1)
        {
            treeDown = false;
            beingChopped = true;
        }
        else 
        {
            treeDown = true;
            beingChopped = false;
        }
    }
}
