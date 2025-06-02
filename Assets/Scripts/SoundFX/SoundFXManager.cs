using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;



public class SoundFXManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioObject;
    public static SoundFXManager instance = null;
   
    private void Awake()
    {
        if (instance == null)
        instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void PlaySoundClip(AudioClip clip, Transform spawn) 
    {
        AudioSource source = Instantiate(audioObject, spawn.position, Quaternion.identity);
        source.clip = clip;
        source.volume = 1f;
        source.Play();
        float clipLength = clip.length;
        Destroy(source.gameObject,clipLength);
    }

    public void PlaySoundClipOnClick(AudioClip clip)
    {
        PlaySoundClip(clip, gameObject.transform);
    }


}
