using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


public class PuzzlePiece : MonoBehaviour
{
    private static float magnet_distance = 0.2f; 
    private static MouseControl MC_ref = MouseControl.GetInstance();
    private static Color hover_light = new Color(0.0f, 0.0f, 0.0f, 0.3f);

    [SerializeField] private int index;
    [SerializeField] private Vector2 center;
    [SerializeField] private Vector2 offset;
    [SerializeField] private Collider2D collision;
    
    private SpriteRenderer SR_ref;
   
    private bool Is_loocking_for_neighbours = false;
    private bool Is_magneting = false;

    private List<ConnectionPoint> neighbours_data;
 
    [SerializeField] private ConnectionPoint closest_connection = null;
    [SerializeField] private List<GameObject> connections;

    private void Awake()
    {
        //collision = gameObject.GetComponent<Collider2D>();
        neighbours_data = new List<ConnectionPoint>();
        SR_ref = gameObject.GetComponent<SpriteRenderer>();
        connections = new List<GameObject>() { gameObject };
    }

    public void SetIndex(int index)
    {
        this.index = index;
    }
    public void SetCenter(Vector2 c) 
    {
        this.center = c;
    }
    public void Set_collider(Collider2D col)
    {
        collision = col;
    }

    private int GetIndex() 
    {
        return index;
    }

    public Vector2 GetCenter() 
    {
        return center;
    }

    public Collider2D GetCollider() 
    {
        return collision;
    }

    public ref List<ConnectionPoint> GetNeighboursData() 
    {
        return ref neighbours_data;
    }
    public void SetNeighboursDataByRef(ref List<ConnectionPoint> r)
    {
        neighbours_data = r;
    }

    public ref List<GameObject> GetConnections()
    {
        return ref connections;
    }
    public void SetConnectionsByRef(ref List<GameObject> r)
    {
        connections = r;
    }

    private void OnMouseEnter()
    {
        if (!MC_ref.Is_holding())
        {
            // add sound effects


            // fading on hover
            SR_ref.color -= hover_light;           
        }
    }
    private void OnMouseExit()
    {
        if (!MC_ref.Is_holding())
        {
            // fading on hover
            SR_ref.color += hover_light;
        }
    }

    private void OnMouseDown()
    {
        MC_ref.SetHoldedPuzzle(this);
        // move to the top <realization>

        offset = Camera.main.ScreenToWorldPoint(Input.mousePosition) - gameObject.transform.position;
        
        
        collision.enabled = false;
        Is_loocking_for_neighbours = true;


    }

    private void OnMouseUp()
    {   
        collision.enabled = true;
        SR_ref.color += hover_light;
        Is_loocking_for_neighbours = false;

        if (Is_magneting) 
        {
            ConnectPuzzle(closest_connection.ReturnGameobjectByCollider().GetComponent<PuzzlePiece>());
            Is_magneting = false;
        }
        
        closest_connection = null;
        MC_ref.UnsetHoldedPuzzle();
    }

    private void OnMouseDrag()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 change = (Vector2)mousePos - (Vector2)gameObject.transform.position - offset;
        // = mousePos - new Vector3 (offset.x, offset.y ,-11);
        //gameObject.transform.position += change;

        if (closest_connection != null)
        {   
            if (Is_magneting)
            {
                if (Vector2.SqrMagnitude(change) > magnet_distance) 
                {
                    //print("move to " + change);
                    MovePuzzle(change);
                    Is_magneting = false;
                }
            }
            else
            {
                MagnetToPuzzle();
            }
        }
        else
        {
            Is_magneting = false;
            MovePuzzle(change);
        }
    }

    public void MovePuzzle(Vector3 change) 
    {
        foreach (var t in neighbours_data)
        {
            t.MovePos(change);
        }
        foreach (var c in connections)
        {
            c.gameObject.transform.position += change;
        }
    }

    private void MagnetToPuzzle() 
    {
        //print("trying magnet to " + closest_connection.Get_index());
        Is_magneting = true;
        Vector2 slide = closest_connection.GetSlide();
        MovePuzzle(slide);
    }

 
    private void ConnectPuzzle(PuzzlePiece p1) 
    {
        print("connecting puzzles " + this.GetIndex() + " and " + p1.GetIndex());

        //adding holded tiles to connections
        foreach (var obj in connections)
        {
            p1.GetConnections().Add(obj);
        }

        foreach (var obj in neighbours_data)
        {
            if (obj.Get_index() != p1.GetIndex())
                p1.GetNeighboursData().Add(obj);
        }
        foreach (var obj in connections) 
        {
            obj.GetComponent<PuzzlePiece>().SetConnectionsByRef(ref p1.GetConnections());
        }


            //p1.GetNeighboursData().RemoveAll(t => t.GetDistance() < magnet_distance);
            p1.GetNeighboursData().RemoveAll(t => connections.Any(p => p.GetComponent<PuzzlePiece>().GetIndex() == t.Get_index()));

        foreach (var obj in connections)
        {   
            obj.GetComponent<PuzzlePiece>().SetNeighboursDataByRef(ref p1.GetNeighboursData());
        }

        //Vector2 slide = closest_connection.GetSlide();
        //MovePuzzle(slide);

    }


   
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (Is_magneting)
            foreach (var t in neighbours_data)
                Gizmos.DrawSphere(t.Get_pos(), 0.05f);
        
    }


    public void CheckCollisionWithNeighbours () 
    {
        float min = -1;
        foreach (var t in neighbours_data)
        {
            if (t.IsPointColliding())
            {
                if ((min < 0 || t.GetDistance() < min) && t.GetDistance() < magnet_distance)
                {
                    min = t.GetDistance();
                    closest_connection = t;
                }
            }

        }
        if (min == -1)
            closest_connection = null;
    }

    private void FixedUpdate()
    {

        if (Is_loocking_for_neighbours)
        {
            CheckCollisionWithNeighbours();
        }

    }



    public void Add_neighbour(int index,Vector2 pos,Collider2D col) 
    {
        foreach(var i in neighbours_data) 
        {
            if (i.Get_index() == index) 
            {
                return;
            }
        }
        var temp_cp = new ConnectionPoint(index, pos,col);
        neighbours_data.Add(temp_cp);
        
    }

    
}

