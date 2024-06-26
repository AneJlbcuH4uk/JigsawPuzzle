using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class ConnectionPoint
{
    [SerializeField] private int puzzle_index;
    private Vector2 releative_position;
    [SerializeField] private Collider2D collider_ref;

    public ConnectionPoint(int index, Vector2 pos, Collider2D colref)
    {
        this.puzzle_index = index;
        this.releative_position = pos;
        this.collider_ref = colref;
    }

    public Vector2 Get_pos() 
    {
        return releative_position;
    }

    public int Get_index() 
    {
        return puzzle_index;
    }
    public void MovePos(Vector2 change) 
    {
        releative_position += change;
    }

    public bool IsPointColliding()
    {
        return collider_ref.OverlapPoint(releative_position);
    }

    public float GetDistance() 
    {
        return Vector2.SqrMagnitude((Vector2)collider_ref.gameObject.transform.position - releative_position);
    }

    public GameObject ReturnGameobjectByCollider() 
    {
        try
        {
            return collider_ref.gameObject;
        }
        catch (NullReferenceException) 
        {
            return null;
        }
    }

    public Vector2 GetSlide() 
    {
        try
        {
            return (Vector2)collider_ref.gameObject.transform.position - releative_position;
        }
        catch (NullReferenceException) 
        {
            return Vector2.zero;
        }
    }
}

