using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GeneralSettings
{
    public string language;
    public bool systemClock;

    public void CreateNew(string l, bool c)
    {
        language = l;
        systemClock = c;
    }

    public override string ToString()
    {
        return "Language set to: " + language + ", SystemClock is active: " + systemClock;
    }

}
