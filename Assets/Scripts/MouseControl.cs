using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MouseControl : MonoBehaviour
{
    [SerializeField] private PuzzlePiece holded_puzzle;
    [SerializeField] private List<GameObject> selected_puzzles = new List<GameObject>();
    private static MouseControl instance;
    private Collider2D force_field;
    private Vector2 offset_for_sel;
    private GameObject SelectionBoxMask;
    private BoxCollider2D MaskCollider;

    
    [SerializeField] private List<GameObject> allpuzzles = new List<GameObject>();
    public static MouseControl GetInstance() 
    {
        if (instance != null)
        {
            return instance;
        }
        return new MouseControl();
    }
    

    public void OnEmptyClick()
    {
        offset_for_sel = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public void OnEmptyDrag()
    {       
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 mask_center = ((Vector2)mousePos - offset_for_sel) / 2 + offset_for_sel;

        SelectionBoxMask.transform.position = new Vector3(mask_center.x,mask_center.y,-2);

        float scale_x = mousePos.x - offset_for_sel.x;
        float scale_y = mousePos.y - offset_for_sel.y;
        SelectionBoxMask.transform.localScale = new Vector3(scale_x, scale_y, 1);
    }

    private void ClearMask()
    {
        SelectionBoxMask.transform.localScale = new Vector3(0, 0, 1);
    }

    Bounds FlattenBounds(Bounds originalBounds)
    {
        originalBounds.min = new Vector3(originalBounds.min.x, originalBounds.min.y, -1);
        originalBounds.max = new Vector3(originalBounds.max.x, originalBounds.max.y,  1);
        return originalBounds;
    }

    public bool ContainsElement(GameObject obj) 
    {
        return selected_puzzles.Contains(obj);
    }

    private void OnEmptyUp() 
    {      
        Bounds boxBounds = FlattenBounds(MaskCollider.bounds);
        if (boxBounds.size.sqrMagnitude == 4) return;

        bool was_added = false;
        foreach(var p in allpuzzles) 
        {
            var col = p.GetComponent<Collider2D>();
            var pp = p.GetComponent<PuzzlePiece>();
            if (boxBounds.Intersects(col.bounds) && !selected_puzzles.Any(element => pp.GetConnections().Contains(element)))
            {
                selected_puzzles.Add(col.gameObject);
                was_added = true;

                col.gameObject.GetComponent<PuzzlePiece>().ChangeOutLineState(true);
            }
        }
        if (!was_added)
        {
            foreach (var p in selected_puzzles)
            {
                p.GetComponent<PuzzlePiece>().ChangeOutLineState(false);
            }

            selected_puzzles.Clear();
        }


        ClearMask();
    }

    public ref List<GameObject> Selected_p() => ref selected_puzzles;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if(instance != this) 
        {
            Destroy(this);
        }
        force_field = gameObject.GetComponent<Collider2D>();

        SelectionBoxMask = GameObject.FindGameObjectWithTag("SelectionBox");
        MaskCollider = SelectionBoxMask.GetComponent<BoxCollider2D>();
    }

    public void SetPuzzles(List<GameObject> data) 
    {
        allpuzzles = data;
    }


    private void Update()
    {
        if (Input.GetButtonDown("Fire1") && !Is_holding())
        {
            OnEmptyClick();          
        }
        if (Input.GetButton("Fire1") && !Is_holding())
        {
            OnEmptyDrag();
        }
        if (Input.GetButtonUp("Fire1") && !Is_holding())
        {    
            OnEmptyUp();
        }

    }


    public void SetHoldedPuzzle(PuzzlePiece p) 
    {
        //print("set");
        holded_puzzle = p;      
        holded_puzzle.MoveToTop();

        var pp = holded_puzzle.GetComponent<PuzzlePiece>();
        if (!selected_puzzles.Any(element => pp.GetConnections().Contains(element)))
        {
            selected_puzzles.Add(holded_puzzle.gameObject);
        }
    }

    public void UnsetHoldedPuzzle() 
    {
        if (holded_puzzle != null)
            holded_puzzle.MoveToBottom();
        holded_puzzle = null;

        foreach(var p in selected_puzzles) 
        {
            p.GetComponent<PuzzlePiece>().ChangeOutLineState(false);
        }
        selected_puzzles.Clear();
    }

    public bool Is_holding() 
    {
        return !System.Object.Equals(holded_puzzle, null);
    }

}
