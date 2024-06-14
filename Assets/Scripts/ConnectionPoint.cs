using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionPoint
{
    private int puzzle_index;
    private Vector2 releative_position;

    public ConnectionPoint(int index, Vector2 pos)
    {
        this.puzzle_index = index;
        this.releative_position = pos;
    }

    public Vector2 Get_pos() 
    {
        return releative_position;
    }

    public int Get_index() 
    {
        return puzzle_index;
    }
}
