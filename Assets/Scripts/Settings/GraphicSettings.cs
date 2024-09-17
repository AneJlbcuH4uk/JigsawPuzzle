using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GraphicSettings
{
    public int FullscreenMode;
    public int ScreenResolutionWidth;
    public int ScreenResolutionHeight;

    public void CreateNew(int Mode, Vector2Int res) 
    {
        FullscreenMode = Mode;
        ScreenResolutionWidth = res.x;
        ScreenResolutionHeight = res.y;
    }

    public override string ToString() 
    {
        return "" + (UnityEngine.FullScreenMode)FullscreenMode + " ScreenResolution " + ScreenResolutionWidth + ":" + ScreenResolutionHeight;
    }

}
