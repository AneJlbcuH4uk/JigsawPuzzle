using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;



public class SoundFXManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioObject;
    public static SoundFXManager instance;
   
    

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }



    public GameObject PlaySoundClip(AudioClip clip, Transform spawn, float volume = 1f, bool loop = false)
    {
        AudioSource source = Instantiate(audioObject, spawn.position, Quaternion.identity);
        source.clip = clip;
        source.volume = volume;
        source.loop = loop;
        source.Play();

        if (!loop) 
        { 
            float clipLength = clip.length;
            Destroy(source.gameObject, clipLength);
        }
        return source.gameObject;
        
    }

    public void PlaySoundClipOnClick(AudioClip clip) 
    {
        PlaySoundClip(clip, gameObject.transform); 
    }


    public GameObject PlaySoundClipOnClick(AudioClip clip, float volume = 1f, bool loop = false)
    {
        return PlaySoundClip(clip, gameObject.transform, volume , loop);
    }

}
