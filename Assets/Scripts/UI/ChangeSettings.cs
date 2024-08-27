using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


public class ChangeSettings : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown fullscreenmode_drop;
    [SerializeField] private TMP_Dropdown aspectratio_drop;
    [SerializeField] private TMP_Dropdown screenresolution_drop;

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

    private static List<Vector2Int> List_of_res_3_2 = new List<Vector2Int>()
    {
        new Vector2Int(1500,1000),
        new Vector2Int(2160,1440),
        new Vector2Int(2560,1700),
        new Vector2Int(3000,2000),
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

    private static List<float> t = new List<float>() { 4 / 3, 3 / 2, 16 / 10, 16 / 9, 21 / 9, -1 };

    private Vector2Int set_to_resol = Vector2Int.zero;
    private string set_to_ratio = "";
    private FullScreenMode set_to_mode = FullScreenMode.ExclusiveFullScreen;

    private void Start()
    {
        ratio_to_res.Add(List_of_res_4_3);
        ratio_to_res.Add(List_of_res_3_2);
        ratio_to_res.Add(List_of_res_16_10);
        ratio_to_res.Add(List_of_res_16_9);
        ratio_to_res.Add(List_of_res_21_9);
        ratio_to_res.Add(List_of_res_others);

        set_to_resol = new Vector2Int(Screen.width, Screen.height);

        print(set_to_resol + " " + (float)set_to_resol.x / (float)set_to_resol.y);
        print(Mathf.Clamp((int)Screen.fullScreenMode - 1, 0, 2));
        print(Screen.fullScreenMode);

        fullscreenmode_drop.value = Mathf.Clamp((int)Screen.fullScreenMode - 1,0,2);

        //int index = t.FindIndex(a => a == (float)set_to_resol.x / (float)set_to_resol.y);
        //aspectratio_drop.value = index;

        
        //print(ratio_to_res[aspectratio_drop.value].FindIndex(a => Vector2.Distance(a, set_to_resol) < 0.01f));

        ChangeResolutionDroptable(0);

        //screenresolution_drop.value = ratio_to_res[aspectratio_drop.value].FindIndex(a => a == set_to_resol);

        
    }

    private void LoadSettingsFromfile() 
    {



    }


    public void OnWindowModeChanged() 
    {
        //
        set_to_mode = mode[fullscreenmode_drop.value];
        fullscreenmode_drop.Hide();
        print(set_to_mode);
    }

    public void OnAspectRatioChange() 
    {
        ChangeResolutionDroptable(aspectratio_drop.value);
        aspectratio_drop.Hide();
        print(aspectratio_drop.captionText.text);
    }

    private void ChangeResolutionDroptable(int index) 
    {
        screenresolution_drop.Hide();
        screenresolution_drop.ClearOptions();

        List<string> options = new List<string>();//{ " --- ---" };

        foreach (Vector2Int obj in ratio_to_res[index]) 
        {
            options.Add("" + obj.x + ':' + obj.y);   
        }
        screenresolution_drop.AddOptions(options);

        set_to_ratio = aspectratio_drop.captionText.text;
        screenresolution_drop.value = 0;
        set_to_resol = ratio_to_res[aspectratio_drop.value][screenresolution_drop.value];
        print(set_to_resol);
    }

    public void OnResolutionChange() 
    {
        if (screenresolution_drop.value >= 0) {
            Vector2Int res = ratio_to_res[aspectratio_drop.value][screenresolution_drop.value];
            set_to_resol = res;
            //Screen.SetResolution(res.x, res.y, fullscreenmode_drop.value != 2);
        }    
    }

    public void OnButtonSave() 
    {
        if(set_to_mode != Screen.fullScreenMode) 
        {
            Screen.fullScreenMode = set_to_mode;
        }
        
        if(set_to_resol != new Vector2Int(Screen.width, Screen.height) && set_to_resol != Vector2Int.zero)
        {
            Screen.SetResolution(set_to_resol.x, set_to_resol.y, mode[fullscreenmode_drop.value]);
        }
    }

}
