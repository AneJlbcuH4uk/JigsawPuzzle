using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GeneralSettings
{
    public string language;
    public bool systemClock;
    public bool autosavetoggle;
    public float autosaveFrequency;
    public int number_of_auto_saves;

    public void CreateNew(string l, bool c, bool at,float af, int noas)
    {
        language = l;
        systemClock = c;
        autosavetoggle = at;
        autosaveFrequency = af;
        number_of_auto_saves = noas;
    }

    public override string ToString()
    {
        return "Language set to: " + language + ", SystemClock is active: " + systemClock + " autosave set to " + autosavetoggle + " frequency " + autosaveFrequency;
    }

}
