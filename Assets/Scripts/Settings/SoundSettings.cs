using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SoundSettings 
{    
    public float GeneralSound;
    public float MusicSound;
    public float EffectsSound;
    public bool MuteApp = false;
    public bool MuteOnMin = false;

    public void CreateNew(float gs,float ms,float es, bool m, bool mom)
    {
        GeneralSound = gs;
        MusicSound = ms;
        EffectsSound = es;
        MuteApp = m;
        MuteOnMin = mom;
    }

    public override string ToString()
    {
        return " GenSound: " + GeneralSound + ", MusicSound: " + MusicSound +
            ", Effects Sound: " + MusicSound + ", App Muted: " + MuteApp + " ,Muted on Minimize: " + MuteOnMin;
    }



}
