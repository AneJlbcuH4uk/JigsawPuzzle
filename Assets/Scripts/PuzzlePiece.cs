using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


public class PuzzlePiece : MonoBehaviour
{
    [SerializeField] private static float magnet_distance = 0.2f;           // distance of puzzle magnet

    private static MouseControl MC_ref;                                     // reference to MouseControl sript instance
    private static Color hover_light = new Color(0.0f, 0.0f, 0.0f, 0.4f);   // fade collor

    [SerializeField] private int index;                 // index of puzzle used in puzzle comparisons
    private Vector2 center;                             // sprite center of puzzle
    private Vector2 offset;                             // variable used to store offset of mouse possition releative to sprite while holding it.
    private Collider2D collision;                       // collider of puzzle used in OnMouseEnter, OnMouseExit .... 
    
    private SpriteRenderer SR_ref;                      // sprite renderer of puzzle keeps cropped image in form of puzzle

    //variables used in detection of neighbours 
    private bool Is_looking_for_neighbours = false;     // if holding puzzle with neighbours in list
    private bool Is_magneting = false;                  // if current holded puzzle is magneted
    private ConnectionPoint closest_connection = null;  // closest puzzle if there is collision was detected

    // lists used to track all possible neighbours and all connected puzzles respectivly
    [SerializeField] private List<ConnectionPoint> neighbours_data;
    private List<GameObject> connections;


    [SerializeField] private AudioClip ConnectionSound;
    [SerializeField] private AudioClip HoverOverSound;
    [SerializeField] private AudioClip PickPuzzleSound;

    [SerializeField] private float ConnectionSound_volume;
    [SerializeField] private float HoverOverSound_volume;
    [SerializeField] private float PickPuzzleSound_volume;

    private static bool is_playing = false;

    public static void change_playing_state(bool state) 
    {
        is_playing = state;
    }

    private PuzzleDataTracker dataTracker;

    private void Awake()
    {
        // init of variables on startup
        neighbours_data = new List<ConnectionPoint>();
        SR_ref = gameObject.GetComponent<SpriteRenderer>();
        connections = new List<GameObject>() { gameObject };
        dataTracker = GameObject.FindGameObjectsWithTag("GameManager")[0].GetComponent<PuzzleDataTracker>();   
    }

    private void Start()
    {
        MC_ref = MouseControl.GetInstance();
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
    public int GetIndex() => index;

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


    private static GameObject SoundInstance = null;

    // adding fading and sound on mouse enter
    private void OnMouseEnter()
    {

        if (!MC_ref.Is_holding())
        {
            if (is_playing)
            {
                if (SoundInstance != null)
                {
                    Destroy(SoundInstance);
                }
                SoundInstance = SoundFXManager.instance.PlaySoundClipOnClick(HoverOverSound, HoverOverSound_volume);


                ChangeOutLineState(true);
            }

        }
    }

    //remove fading on mouse exit
    private void OnMouseExit()
    {
        if (!MC_ref.Is_holding())
        {
            // fading on hover

            if (!MC_ref.ContainsElement(gameObject))
                ChangeOutLineState(false);

        }
    }

    // taking puzzle as holded to check it for connections
    private void OnMouseDown()
    {
        if (dataTracker.IsInteractionDisabled())
            return;

        MC_ref.SetHoldedPuzzle(this);
        //----------------------- play sound ----------------------- 
        if (is_playing)
        {
            if (SoundInstance != null)
            {
                Destroy(SoundInstance);
            }
            SoundInstance = SoundFXManager.instance.PlaySoundClipOnClick(PickPuzzleSound, PickPuzzleSound_volume);
        }
        //----------------------------------------------------------

        offset = Camera.main.ScreenToWorldPoint(Input.mousePosition) - gameObject.transform.position;


        ChangeCollisionState(false);

        foreach (var puz in MC_ref.Selected_p())
        {
            var con = puz.GetComponent<PuzzlePiece>().GetConnections();
            foreach (var t in con)
            {
                t.GetComponent<PuzzlePiece>().ChangeCollisionState(false);
            }
        }

        Is_looking_for_neighbours = true;


    }


    //connect magneted puzzle to <connections> or if not magneted remove puzzle from holded
    private void OnMouseUp()
    {
        ChangeCollisionState(true);

        foreach (var puz in MC_ref.Selected_p())
        {
            var con = puz.GetComponent<PuzzlePiece>().GetConnections();
            foreach (var t in con)
            {
                t.GetComponent<PuzzlePiece>().ChangeCollisionState(true);
            }
        }
        SR_ref.color += hover_light;
        Is_looking_for_neighbours = false;

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
        Vector3 mousePos = InputControl.instance.limit_camera_pos3(Camera.main.ScreenToWorldPoint(Input.mousePosition),0);
        Vector3 change = (Vector2)mousePos - (Vector2)gameObject.transform.position - offset;

        // magnetting puzzle logic
        if (closest_connection != null && MC_ref.Selected_p().Count < 2)
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

    // do not use transform.position+= or any other method to move puzzles
    public void MovePuzzle(Vector3 change) 
    {
        foreach (var puz in MC_ref.Selected_p())
        {
            var con = puz.GetComponent<PuzzlePiece>().GetConnections();
            var nd = puz.GetComponent<PuzzlePiece>().GetNeighboursData();

            foreach (var t in nd)
            {
                t.MovePos(change);
            }
            foreach (var c in con)
            {
                c.transform.position += change;
            }
        }
    }

    public void ChangeOutLineState(bool state) 
    {
        foreach(var t in connections) 
        {
            t.transform.GetChild(0).gameObject.SetActive(state);
        }
    }


    // magnet to required possition of clossest neighbour.
    private void MagnetToPuzzle() 
    {
        if (MC_ref.Selected_p().Count > 1) return;
        Is_magneting = true;
        Vector2 slide = closest_connection.GetSlide();
        MovePuzzle(slide);
    }

    private void ConnectPuzzle(PuzzlePiece p1, bool enable_sound = true)
    {
        //----------------------- play sound ----------------------- 
        if (enable_sound && is_playing) { 
            if (SoundInstance != null)
            {
                Destroy(SoundInstance);
            }
            SoundFXManager.instance.PlaySoundClipOnClick(ConnectionSound, ConnectionSound_volume);
        }
        //----------------------------------------------------------
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
        if (Is_looking_for_neighbours)
        {
            CheckCollisionWithNeighbours();
        }   
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

    public void ConnectOnLoad(Vector3 pos) 
    {
        float md_temp = magnet_distance;
        magnet_distance = 0.0001f;
        if (dataTracker.IsInteractionDisabled())
            return;

        MC_ref.SetHoldedPuzzle(this);

        ChangeCollisionState(false);       
        MovePuzzle(pos - (Vector3)GetCenter());
        ChangeCollisionState(true);

        for (int i = 0; i < neighbours_data.Count; i++)
        {
            CheckCollisionWithNeighbours();
            if (closest_connection != null)
                ConnectPuzzle(closest_connection.ReturnGameobjectByCollider().GetComponent<PuzzlePiece>(),false);
            else
                break;
        }
        closest_connection = null;
        MC_ref.UnsetHoldedPuzzle();
        magnet_distance = md_temp;
    }

    
}

