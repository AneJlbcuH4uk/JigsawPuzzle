using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePiece : MonoBehaviour
{
    [SerializeField] private int index;
    [SerializeField] public List<ConnectionPoint> neighbours_pos;

    [SerializeField] private Vector2 offset;

    private void Awake()
    {
        neighbours_pos = new List<ConnectionPoint>();
    }

    public void SetIndex(int index)
    {
        this.index = index;
    }
    private void OnMouseEnter()
    {
        var s = "";
        foreach(var i in neighbours_pos)
        {
            s += i.Get_index() + "  ";
        }
        print("hover over " + index + " puzzle list of neighbours: " + s);
    }
    private void OnMouseExit()
    {

    }

    private void OnMouseDown()
    {
        // move to the top <realization>
        offset = Camera.main.ScreenToWorldPoint(Input.mousePosition) - gameObject.transform.position;
    }

    private void OnMouseDrag()
    {
        gameObject.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3 (offset.x, offset.y ,11);
    }

    public void Add_neighbour(int index,Vector2 pos) 
    {
        foreach(var i in neighbours_pos) 
        {
            if (i.Get_index() == index) 
            {
                return;
            }
        }
        var temp = new ConnectionPoint(index, pos);
        neighbours_pos.Add(temp);
    }

}

