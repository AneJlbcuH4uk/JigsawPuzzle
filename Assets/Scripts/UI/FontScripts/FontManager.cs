using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;


public class FontManager : MonoBehaviour
{
    [SerializeField] private TMP_FontAsset JP;
    [SerializeField] private TMP_FontAsset CH;
    [SerializeField] private TMP_FontAsset KO;
    [SerializeField] private TMP_FontAsset EU;

    private static FontManager instance;
    private string cur_language = "eu";


    TMP_FontAsset GetFontByType(string t) 
    {
        switch (t) 
        {
            case "ja": return JP;
            case "zh": return CH;
            case "ko": return KO;
            default: return EU;
        }
    }

    public static FontManager GetInstance() 
    {
        if (instance == null)
            return new FontManager();
        else
            return instance;
    }

    private List<TMP_Text> label_list = new List<TMP_Text>();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        instance = this;        
    }

    private void Start()
    {
        var _Main_canvas = GameObject.FindGameObjectWithTag("MainCanvas");
        GeneralSettings _GeneralSettings = _Main_canvas.GetComponent<SettingsInit>().ReloadGeneralSettings();
        cur_language = _GeneralSettings.language;
    }

    public void ChangeFont(string locale) 
    {
        var cur_font = GetFontByType(locale);
        cur_language = locale;
        foreach (var label in label_list) 
        {
            label.font = cur_font;
        }
    }

    public void UppdateFontForEnabled()
    {
        var cur_font = GetFontByType(cur_language);
        foreach (var label in label_list)
        {
            label.font = cur_font;
        }
    }


    public void Addlabellistener(GameObject l)
    {
        var t = l.GetComponent<TMP_Text>();
        if (t == null) return;
        label_list.Add(t);
    }



}
