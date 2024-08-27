using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


public class PuzzlePiece : MonoBehaviour
{
    [SerializeField] private static float magnet_distance = 0.2f;           // distance of puzzle magnet

    private static MouseControl MC_ref = MouseControl.GetInstance();        // reference to MouseControl sript instance
    private static Color hover_light = new Color(0.0f, 0.0f, 0.0f, 0.4f);   // fade collor

    [SerializeField] private int index;                 // index of puzzle used in puzzle comparisons
    private Vector2 center;                             // sprite center of puzzle
    private Vector2 offset;                             // variable used to store offset of mouse possition releative to sprite while holding it.
    private Collider2D collision;                       // collider of puzzle used in OnMouseEnter, OnMouseExit .... 
    
    private SpriteRenderer SR_ref;                      // sprite renderer of puzzle keeps cropped image in form of puzzle

    //variables used in detection of neighbours 
    private bool Is_loocking_for_neighbours = false;    // if holding puzzle with neighbours in list
    private bool Is_magneting = false;                  // if current holded puzzle is magneted
    private ConnectionPoint closest_connection = null;  // closest puzzle if there is collision was detected

    // lists used to track all possible neighbours and all connected puzzles respectivly
    [SerializeField] private List<ConnectionPoint> neighbours_data;
    private List<GameObject> connections;
    
    
    private PuzzleDataTracker dataTracker;
    //private SpriteRenderer Thickness_ref;

    private void Awake()
    {
        // init of variables on startup
        neighbours_data = new List<ConnectionPoint>();
        SR_ref = gameObject.GetComponent<SpriteRenderer>();
        connections = new List<GameObject>() { gameObject };
        dataTracker = GameObject.FindGameObjectsWithTag("GameManager")[0].GetComponent<PuzzleDataTracker>();
        //Thickness_ref = transform.GetChild(0).GetComponent<SpriteRenderer>();
        
    }

    //setters
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
    public void SetNeighboursDataByRef(ref List<ConnectionPoint> r)
    {
        neighbours_data = r;
    }
    public void SetConnectionsByRef(ref List<GameObject> r)
    {
        connections = r;
    }

    // getters
    private int GetIndex() => index;

    public Vector2 GetCenter() => center;

    public Collider2D GetCollider() => collision;

    public ref List<ConnectionPoint> GetNeighboursData() => ref neighbours_data;
    
    public ref List<GameObject> GetConnections() => ref connections; 
   
    public void MoveToTop() 
    {
        foreach(var obj in connections) 
        {
            obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y, -1);
        }
    }

    public void MoveToBottom()
    {
        foreach (var obj in connections)
        {
            obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y, 1);
        }
    }

    // currently only adding fading on mouse enter of collider
    private void OnMouseEnter()
    {
        //if (dataTracker.IsInteractionDisabled())
        //    return;

        if (!MC_ref.Is_holding())
        {
            // add sound effects

            //print("entered" + (SR_ref.color.a == 1));
            // fading on hover
            if (1 - SR_ref.color.a < hover_light.a)
            {
                SR_ref.color -= hover_light;
                //Thickness_ref.color -= hover_light;
            }

        }
    }

    // currently only remove fading on mouse exit of collider
    private void OnMouseExit()
    {
        //if (dataTracker.IsInteractionDisabled())
        //    return; 


        if (!MC_ref.Is_holding())
        {
            // fading on hover
            if (SR_ref.color.a < 1) 
            { 
                SR_ref.color += hover_light;
                //Thickness_ref.color += hover_light;
            }

        }
    }

    // taking puzzle as holded to check it for connections
    private void OnMouseDown()
    {
        if (dataTracker.IsInteractionDisabled())
            return;

        MC_ref.SetHoldedPuzzle(this);
        offset = Camera.main.ScreenToWorldPoint(Input.mousePosition) - gameObject.transform.position;


        ChangeCollisionState(false);
        foreach (var t in connections)
        {
            t.GetComponent<PuzzlePiece>().ChangeCollisionState(false);
        }

        Is_loocking_for_neighbours = true;
    }

    //connect magneted puzzle to <connections> or if not magneted remove puzzle from holded
    private void OnMouseUp()
    {
        ChangeCollisionState(true);
        foreach(var t in connections) 
        {
            t.GetComponent<PuzzlePiece>().ChangeCollisionState(true);
        }

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

    public void ChangeCollisionState(bool state) 
    {
        collision.enabled = state;
    }


    private void OnMouseDrag()
    {
        if (dataTracker.IsInteractionDisabled())
            return;
        // getting position of mouse and change of the moved object
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 change = (Vector2)mousePos - (Vector2)gameObject.transform.position - offset;

        // magnetting puzzle logic
        if (closest_connection != null)
        {   
            if (Is_magneting)
            {
                if (Vector2.SqrMagnitude(change) > magnet_distance) 
                {
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

    // do not use transform.position+= or any other method to meve puzzles
    // used for puzzle moving(moves all connected puzzles) --- need to be optimized ------------------------- (11)
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

    // magnet to required possition of clossest neighbour.
    private void MagnetToPuzzle() 
    {
        Is_magneting = true;
        Vector2 slide = closest_connection.GetSlide();
        MovePuzzle(slide);
    }

 
    private void ConnectPuzzle(PuzzlePiece p1) 
    {
        //print("connecting puzzles " + this.GetIndex() + " and " + p1.GetIndex());

        //adding holded tiles to connections
        foreach (var obj in connections)
        {
            p1.GetConnections().Add(obj);
        }

        //adding non repetetive neighbours to list
        foreach (var obj in neighbours_data)
        {
            if (obj.Get_index() != p1.GetIndex())
                p1.GetNeighboursData().Add(obj);
        }

        // setting in each of connected puzzles list of connections to newly created one.
        foreach (var obj in connections) 
        {
            obj.GetComponent<PuzzlePiece>().SetConnectionsByRef(ref p1.GetConnections());
        }

        // removing already connected puzzles from list of possible connections in case when connected puzzles have few connections with <connections>.
        p1.GetNeighboursData().RemoveAll(t => connections.Any(p => p.GetComponent<PuzzlePiece>().GetIndex() == t.Get_index()));

        // setting in each of connected puzzles list of possible connections to newly created one.
        foreach (var obj in connections)
        {   
            obj.GetComponent<PuzzlePiece>().SetNeighboursDataByRef(ref p1.GetNeighboursData());
        }

        dataTracker.SetMaxComb(connections.Count);
    }


    // method sets <clossest connection> to puzzle which closest to holded. sets null if no collision with neigbours detected.
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
        // checking if holded puzzle collides with neighbours via connection point
        if (Is_loocking_for_neighbours)
        {
            CheckCollisionWithNeighbours();
        }
        //if (dataTracker.IsInteractionDisabled() && SR_ref.color.a < 1) 
        //{
        //    SR_ref.color += hover_light;
        //}
                
    }


    // method adding neighbour to list, not adding if neighbour with same index already exists
    // do not recoment call it outside of puzzle generation
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

