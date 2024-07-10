using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class MouseControl : MonoBehaviour
{
    [SerializeField] private PuzzlePiece holded_puzzle;

    private static MouseControl instance;
    private Collider2D force_field;

    public static MouseControl GetInstance() 
    {
        if (instance == null) 
        {
            instance = new MouseControl();
        }
        return instance;
    }

   

    private void Start()
    {
        force_field = gameObject.GetComponent<Collider2D>();
        instance = this;
    }

    //private void Update()
    //{
    //    if (holded_puzzle != null)
    //    {
    //        holded_puzzle.CheckCollisionWithNeighbours();
    //    }
    //}

    //private void FixedUpdate()
    //{


    //    //if (Input.GetButton("Fire2")) 
    //    //{
    //    //    var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //    //    force_field.offset = mouseWorldPos;
    //    //    force_field.enabled = true;

    //    //}
    //    //else
    //    //    force_field.enabled = false;
    //}

    public void SetHoldedPuzzle(PuzzlePiece p) 
    {
        holded_puzzle = p;
    }

    public void UnsetHoldedPuzzle() 
    {
        holded_puzzle = null;
    }

    public bool Is_holding() 
    {
        return !System.Object.Equals(holded_puzzle, null);
    }
}
