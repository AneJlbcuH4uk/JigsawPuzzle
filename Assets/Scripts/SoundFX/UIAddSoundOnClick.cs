using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAddSoundOnClick : MonoBehaviour
{

    [SerializeField] AudioClip clip;
    private Button button;
    private Toggle toggle;


    void Awake()
    {
        
        try 
        {
            button = gameObject.GetComponent<Button>();
            button.onClick.AddListener(() => SoundFXManager.instance.PlaySoundClipOnClick(clip));
        }
        catch 
        {
            
        }

        try 
        {
            toggle = gameObject.GetComponent<Toggle>();
            toggle.onValueChanged.AddListener((bool val) => SoundFXManager.instance.PlaySoundClipOnClick(clip));
        }
        catch 
        {

        }
    }

}
