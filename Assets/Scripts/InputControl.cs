using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] private float speed = 5; // speed of camear movement while cursore close to the window edge
    [SerializeField] private int camera_border_width = 20; // width in pixels how close cursor to edge must be in order to move camera
    [SerializeField] private float scale = 0.2f; // speed of getting camera up via scrolling
    [SerializeField] private InGameUi EscapeMenuUI;
    [SerializeField] private Vector2 zoom_limits = new Vector2 (3,20);

    private Camera camera;  // instance of main scene camera
    
    private Vector3 offset; 
    private Vector2 change;

    // declaring of event which called on screen size changed
    public delegate void ScreenSizeChangeEventHandler(int Width, int Height);       
    public event ScreenSizeChangeEventHandler ScreenSizeChangeEvent;

    private PuzzleGeneration GameManager;

    protected virtual void OnScreenSizeChange(int Width, int Height)
    {              
        if (ScreenSizeChangeEvent != null) 
            ScreenSizeChangeEvent(Width, Height);
    }

    // variables for event
    private Vector2 lastScreenSize;
    public static CameraControl instance = null;



    private void Awake()
    {
        // initializing variables
        camera = gameObject.GetComponent<Camera>();
        offset = Vector2.zero;
        change = Vector2.zero;
        lastScreenSize = new Vector2(Screen.width, Screen.height);
        instance = this;

        GameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<PuzzleGeneration>();
    }

    void Update()
    {
        if (GameManager.Is_Loading()) 
        {
            return;
        }

        // camera scroll 
        if (Mathf.Abs(Input.mouseScrollDelta.y) > 0.001)
        {
            float change = scale * Input.mouseScrollDelta.y;
            if (camera.orthographicSize - change <= zoom_limits.y &&
                camera.orthographicSize - change >= zoom_limits.x)
            {
                camera.orthographicSize -= change;
            }
        }

        // getting position of cursor on mouse button down needed for next if statement
        if (Input.GetButtonDown("MoveCamera") && !EscapeMenuUI.IsUIActive())
        {
            offset = camera.ScreenToWorldPoint(Input.mousePosition);
        }

        // using <offset> moving camera with holded mouse button
        if (Input.GetButton("MoveCamera") && !EscapeMenuUI.IsUIActive())
        {
            change = - camera.ScreenToWorldPoint(Input.mousePosition) + (Vector3)offset + gameObject.transform.position;
            gameObject.transform.position = new Vector3(change.x, change.y, -10);         
        }

        if (Input.GetButtonDown("Cancel")) 
        {
            EscapeMenuUI.Pause();
        }


        // moving camera if cursor close to window edge
        if (IsCursorAtTheBorder() && !EscapeMenuUI.IsUIActive()) 
        {
            gameObject.transform.position += (Vector3)((Vector2)Input.mousePosition - lastScreenSize / 2).normalized * speed * Time.deltaTime;
        }
    }

    
    //drawing border for camera in scene view
    //private void OnDrawGizmos()
    //{
    //    if (instance != null)
    //    {
    //        Gizmos.color = Color.red;
    //        Gizmos.DrawLine(camera.ScreenToWorldPoint(new Vector3(camera_border_width, 0, 0)), camera.ScreenToWorldPoint(new Vector3(camera_border_width, lastScreenSize.y, 0)));
    //        Gizmos.DrawLine(camera.ScreenToWorldPoint(new Vector3(lastScreenSize.x - camera_border_width, 0, 0)), camera.ScreenToWorldPoint(new Vector3(lastScreenSize.x - camera_border_width, lastScreenSize.y, 0)));
    //        Gizmos.DrawLine(camera.ScreenToWorldPoint(new Vector3(0, camera_border_width, 0)), camera.ScreenToWorldPoint(new Vector3(lastScreenSize.x, camera_border_width, 0)));
    //        Gizmos.DrawLine(camera.ScreenToWorldPoint(new Vector3(0, lastScreenSize.y - camera_border_width, 0)), camera.ScreenToWorldPoint(new Vector3(lastScreenSize.x, lastScreenSize.y - camera_border_width, 0)));
    //    }
    //}

    private void FixedUpdate()
    {
        // tracking and updating screen size for proper working of camera movement while geting close to border in case of screen resize

        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        if (this.lastScreenSize != screenSize)
        {
            this.lastScreenSize = screenSize;
            OnScreenSizeChange(Screen.width, Screen.height);
        }
        //
    }


    // bool check if cursor close to window border
    private bool IsCursorAtTheBorder() 
    {
        //return false;   //this line needed to disable movement of camera while cursor near border 
        return Input.mousePosition.x < camera_border_width ||
               Input.mousePosition.x > lastScreenSize.x - camera_border_width ||
               Input.mousePosition.y < camera_border_width ||
               Input.mousePosition.y > lastScreenSize.y - camera_border_width;
    }



}
