using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePiece : MonoBehaviour
{
    [SerializeField] private int index;
    [SerializeField] public List<ConnectionPoint> neighbours_pos;

    [SerializeField] private Vector2 offset;

    public void SetIndex(int index)
    {
        this.index = index;
    }
    private void OnMouseEnter()
    {
        print("hover over " + index + " puzzle");
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
}

