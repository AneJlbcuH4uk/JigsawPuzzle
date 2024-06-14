using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MouseControl : MonoBehaviour
{
    Collider2D force_field;
    private void Start()
    {
        force_field = gameObject.GetComponent<Collider2D>();
    }
    private void Update()
    {

        if (Input.GetButton("Fire2")) 
        {
            //Vector3 mousePos = Input.mousePosition;
            var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            force_field.offset = mouseWorldPos;

            force_field.enabled = true;
        }
        else
            force_field.enabled = false;
    }
}
