using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


public class ChangeGraphicsSettings : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown fullscreenmode_drop;
    [SerializeField] private TMP_Dropdown aspectratio_drop;
    [SerializeField] private TMP_Dropdown screenresolution_drop;
    [SerializeField] private GameObject Confirmation_screen;
    [SerializeField] private GameObject CGS_Canvas;
    [SerializeField] private UIBehaviour UIB;

    [SerializeField] private SettingsInit config;

    [SerializeField] private bool SettingsWasChanged = false;

    private static List<FullScreenMode> mode = new List<FullScreenMode>()
    {
        FullScreenMode.ExclusiveFullScreen ,
        FullScreenMode.MaximizedWindow ,
        FullScreenMode.Windowed 
    };

    private static List<Vector2Int> List_of_res_4_3 = new List<Vector2Int>()
    {
        new Vector2Int(640,480),
        new Vector2Int(800,600),
        new Vector2Int(960,720),
        new Vector2Int(1024,768),
        new Vector2Int(1280,960),
        new Vector2Int(1400,1050),
        new Vector2Int(1440,1080),
        new Vector2Int(1600,1200),
        new Vector2Int(1856,1392),
        new Vector2Int(1920,1440),
        new Vector2Int(2048,1536)
    };

   
    private static List<Vector2Int> List_of_res_16_10 = new List<Vector2Int>()
    {
        new Vector2Int(1280,800),
        new Vector2Int(1440,900),
        new Vector2Int(1680,1050),
        new Vector2Int(1920,1200),
        new Vector2Int(2560,1600)
    };

    private static List<Vector2Int> List_of_res_16_9 = new List<Vector2Int>()
    {
        new Vector2Int(1024,576),
        new Vector2Int(1152,648),
        new Vector2Int(1280,720),
        new Vector2Int(1366,768),
        new Vector2Int(1600,900),
        new Vector2Int(1920,1080),
        new Vector2Int(2560,1440),
        new Vector2Int(3840,2160)
    };

    private static List<Vector2Int> List_of_res_21_9 = new List<Vector2Int>()
    {
        new Vector2Int(2560,1080),
        new Vector2Int(3440,1440)
    };

    private static List<Vector2Int> List_of_res_others = new List<Vector2Int>();

    private static List<List<Vector2Int>> ratio_to_res = new List<List<Vector2Int>>();

    private static List<float> t = new List<float>() { 4f / 3f, 16f / 10f, 16f / 9f, 21f / 9f, -1f };

    private GraphicSettings GraphicSettings;

    //private Vector2Int set_to_resol = Vector2Int.zero;
    private string set_to_ratio = "";
    //private FullScreenMode set_to_mode = FullScreenMode.ExclusiveFullScreen;


    private void Awake()
    {
        ratio_to_res.Clear();
        UIB = GameObject.FindGameObjectWithTag("MainCanvas").GetComponent<UIBehaviour>();
        
        ratio_to_res.Add(List_of_res_4_3);
        ratio_to_res.Add(List_of_res_16_10);
        ratio_to_res.Add(List_of_res_16_9);
        ratio_to_res.Add(List_of_res_21_9);
        ratio_to_res.Add(List_of_res_others);

        Open();
    }

    bool isEqual(float a, float b)
    {
        float eps = 0.01f;
        if (Mathf.Abs(a - b) <= eps)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Open() 
    {
        RestoreSettings();
        UIB.RefreshAnimationCounter();
    }

    public void RestoreSettings() 
    {
        
        if (config == null) 
        {
            config = GameObject.FindWithTag("MainCanvas").GetComponent<SettingsInit>();
            //print(config + "  WHY???");
        }

        GraphicSettings = config.ReloadGraphicsSettings();

        fullscreenmode_drop.value = GraphicSettings.FullscreenMode;

        int index = t.FindIndex(a => isEqual(a, (float)GraphicSettings.ScreenResolutionWidth / (float)GraphicSettings.ScreenResolutionHeight));
        if (index == -1)
            index = ratio_to_res.Count - 1;

        Vector2Int cur_res = new Vector2Int(GraphicSettings.ScreenResolutionWidth, GraphicSettings.ScreenResolutionHeight);
        int res_ind = ratio_to_res[index].FindIndex(a => a == cur_res);
        if (res_ind == -1)
            res_ind = 0;

        aspectratio_drop.value = index;
        ChangeResolutionDroptable(index, res_ind);

        SettingsWasChanged = false;
    }


    public void OnWindowModeChanged() 
    {
        GraphicSettings.FullscreenMode = Mathf.Clamp((int)mode[fullscreenmode_drop.value] - 1,0,2);
        //set_to_mode = mode[fullscreenmode_drop.value];
        fullscreenmode_drop.Hide();
        //print(set_to_mode);
        SettingsWasChanged = true;
    }

    public void OnAspectRatioChange() 
    {
        ChangeResolutionDroptable(aspectratio_drop.value);
        aspectratio_drop.Hide();
        //print(aspectratio_drop.captionText.text);
    }

    private void ChangeResolutionDroptable(int index, int res_ind = 0) 
    {
        screenresolution_drop.Hide();
        screenresolution_drop.ClearOptions();

        List<string> options = new List<string>();//{ "--- ---" };

        foreach (Vector2Int obj in ratio_to_res[index]) 
        {
            options.Add("" + obj.x + ':' + obj.y);
        }
        screenresolution_drop.AddOptions(options);

        set_to_ratio = aspectratio_drop.captionText.text;
        screenresolution_drop.value = res_ind;

        GraphicSettings.ScreenResolutionWidth = ratio_to_res[aspectratio_drop.value][screenresolution_drop.value].x;
        GraphicSettings.ScreenResolutionHeight = ratio_to_res[aspectratio_drop.value][screenresolution_drop.value].y;

        //set_to_resol = ratio_to_res[aspectratio_drop.value][screenresolution_drop.value];
        //print(set_to_resol);
    }

    public void OnResolutionChange() 
    {
        if (screenresolution_drop.value >= 0) {
            Vector2Int res = ratio_to_res[aspectratio_drop.value][screenresolution_drop.value];
            GraphicSettings.ScreenResolutionWidth = res.x;
            GraphicSettings.ScreenResolutionHeight = res.y;
            //Screen.SetResolution(res.x, res.y, fullscreenmode_drop.value != 2);
        }
        SettingsWasChanged = true;

        
    }

    public void OnButtonSave() 
    {
        if (!SettingsWasChanged) return;

        if (GraphicSettings.FullscreenMode != (int)Screen.fullScreenMode) 
        {
            Screen.fullScreenMode = (FullScreenMode)Mathf.Clamp(GraphicSettings.FullscreenMode == 0 ? GraphicSettings.FullscreenMode : GraphicSettings.FullscreenMode + 1,0,3);
        }
        
        if(GraphicSettings.ScreenResolutionWidth != Screen.width
            && GraphicSettings.ScreenResolutionHeight != Screen.height
            && GraphicSettings.ScreenResolutionWidth != 0 
            && GraphicSettings.ScreenResolutionHeight != 0)
        {
            Screen.SetResolution(GraphicSettings.ScreenResolutionWidth, GraphicSettings.ScreenResolutionHeight, mode[fullscreenmode_drop.value]);
        }

        config.SaveSettings(GraphicSettings, Settings.Graphics);
        SettingsWasChanged = false;
    }
    public void OnExitWithoutConfirmation()
    {
        if (SettingsWasChanged)
        {
            Confirmation_screen.SetActive(true);
            ChangeNoSaveInMenu();
        }
        else
        {
            UIB.SubtractAnimationCounter();
        }
    }
    public void CancelChanges()
    {
        RestoreSettings();
    }

    public void ChangeNoSaveInMenu()
    {
        UIB.unsaved_in_menu = true;
    }
}
