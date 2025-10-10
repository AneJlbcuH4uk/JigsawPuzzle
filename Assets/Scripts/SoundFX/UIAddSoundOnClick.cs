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

        button = GetComponent<Button>();
        toggle = GetComponent<Toggle>();

        if (button != null)
        {
            button.onClick.AddListener(() => SoundFXManager.instance.PlaySoundClipOnClick(clip));
        }
        else if (toggle != null)
        {
            toggle.onValueChanged.AddListener((bool val) => SoundFXManager.instance.PlaySoundClipOnClick(clip));
        }
        else
        {
            Debug.LogError($"{name} must have either a Button or Toggle component attached!", this);
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
