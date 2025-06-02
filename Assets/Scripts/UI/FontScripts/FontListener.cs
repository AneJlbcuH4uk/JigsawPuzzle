using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FontListener : MonoBehaviour
{
    private FontManager fm;


    private void Awake()
    {
        fm = FontManager.GetInstance();
        fm.Addlabellistener(gameObject);
    }

    void OnEnable()
    {
        // Apply the current language font when enabled
        FontManager.GetInstance().UppdateFontForEnabled();
    }

    //private void Start()
    //{
    //    fm.Addlabellistener(gameObject);
    //}


}
