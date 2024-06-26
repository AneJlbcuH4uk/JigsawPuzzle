using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] private float speed = 5;
    [SerializeField] private int camera_border_width = 20;

    private Camera camera;
    private float scale = 0.2f;
    private Vector3 offset;
    private Vector2 change;

    public delegate void ScreenSizeChangeEventHandler(int Width, int Height);       
    public event ScreenSizeChangeEventHandler ScreenSizeChangeEvent;                
    protected virtual void OnScreenSizeChange(int Width, int Height)
    {              
        if (ScreenSizeChangeEvent != null) 
            ScreenSizeChangeEvent(Width, Height);
    }

    private Vector2 lastScreenSize;
    public static CameraControl instance = null;



    private void Awake()
    {
        camera = gameObject.GetComponent<Camera>();
        offset = Vector2.zero;
        change = Vector2.zero;

        print(camera.orthographicSize);

        lastScreenSize = new Vector2(Screen.width, Screen.height);
        instance = this;

    }

    void Update()
    {
        if (Mathf.Abs(Input.mouseScrollDelta.y) > 0.1)
            camera.orthographicSize -= scale * Input.mouseScrollDelta.y;

        if (Input.GetButtonDown("MoveCamera"))
        {
            offset = camera.ScreenToWorldPoint(Input.mousePosition);
        }


        if (Input.GetButton("MoveCamera"))
        {
            change = - camera.ScreenToWorldPoint(Input.mousePosition) + (Vector3)offset + gameObject.transform.position;
            gameObject.transform.position = new Vector3(change.x, change.y, -10);         
        }

        if (IsCursorAtTheBorder()) 
        {
            gameObject.transform.position += (Vector3)((Vector2)Input.mousePosition - lastScreenSize / 2).normalized * speed * Time.deltaTime;
        }
        //print(IsCursorAtTheBorder());
    }

    private void OnDrawGizmos()
    {
        if (instance != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(camera.ScreenToWorldPoint(new Vector3(camera_border_width, 0, 0)), camera.ScreenToWorldPoint(new Vector3(camera_border_width, lastScreenSize.y, 0)));
            Gizmos.DrawLine(camera.ScreenToWorldPoint(new Vector3(lastScreenSize.x - camera_border_width, 0, 0)), camera.ScreenToWorldPoint(new Vector3(lastScreenSize.x - camera_border_width, lastScreenSize.y, 0)));
            Gizmos.DrawLine(camera.ScreenToWorldPoint(new Vector3(0, camera_border_width, 0)), camera.ScreenToWorldPoint(new Vector3(lastScreenSize.x, camera_border_width, 0)));
            Gizmos.DrawLine(camera.ScreenToWorldPoint(new Vector3(0, lastScreenSize.y - camera_border_width, 0)), camera.ScreenToWorldPoint(new Vector3(lastScreenSize.x, lastScreenSize.y - camera_border_width, 0)));
        }
    }

    private void FixedUpdate()
    {
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        if (this.lastScreenSize != screenSize)
        {
            this.lastScreenSize = screenSize;
            OnScreenSizeChange(Screen.width, Screen.height);
        }
    }

    private bool IsCursorAtTheBorder() 
    {
        return false;
        return Input.mousePosition.x < camera_border_width ||
               Input.mousePosition.x > lastScreenSize.x - camera_border_width ||
               Input.mousePosition.y < camera_border_width ||
               Input.mousePosition.y > lastScreenSize.y - camera_border_width;
    }



}
