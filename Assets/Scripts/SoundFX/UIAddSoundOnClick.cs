using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAddSoundOnClick : MonoBehaviour
{

    [SerializeField] AudioClip clip;

    private AudioClip clip_backup;
    private Button button;
    private Toggle toggle;


    void Awake()
    {
        clip_backup = clip;
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

    public void ChangeSoundClip() 
    {
        var new_clip = GetComponent<AdditionalAudioClip>().GetClip();
        clip = new_clip;
    }

    public void RestoreClip()
    {
        if(clip != clip_backup)
            clip = clip_backup;
    }

}
