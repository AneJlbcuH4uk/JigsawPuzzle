using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;

public class PuzzleDataTracker : MonoBehaviour
{
    private int number_of_puzzles_in_scene;
    private int current_max_of_combined_puzzles = 1;
    private Material image_mat;

    [SerializeField] private float auto_save_delay = 60f;
    [SerializeField] private bool interaction_disabled = false;
    [SerializeField] private bool isAutoSaving = false;
    [SerializeField] private int number_of_autosaves = 10;
    [SerializeField] private UIBehaviour UIB_ref;

    private string save_path;
    private GeneralSettings _GeneralSettings;

    PuzzleGeneration pg;
    

    public bool IsInteractionDisabled() => interaction_disabled;
    public void SetInteractionBool(bool set) 
    {
        interaction_disabled = set;
    }

    private void Awake()
    {
        image_mat = GetComponent<SpriteRenderer>().material;
        image_mat.SetFloat("_StartAnim", 0);
        image_mat.SetFloat("_CurAnimSecond", 0);

        pg = GetComponent<PuzzleGeneration>();


        var _Main_canvas = GameObject.FindGameObjectWithTag("MainCanvas");
        _GeneralSettings = _Main_canvas.GetComponent<SettingsInit>().ReloadGeneralSettings();        

        //print(_GeneralSettings);

        isAutoSaving = _GeneralSettings.autosavetoggle;
        auto_save_delay = _GeneralSettings.autosaveFrequency * 30;
        number_of_autosaves = _GeneralSettings.number_of_auto_saves;
        print($"autosave is {isAutoSaving} delay between saves {auto_save_delay} number of autosaves {number_of_autosaves}");

        
        if (isAutoSaving == true) StartCoroutine(AutoSave());

        
    }

    IEnumerator AutoSave() 
    {
        yield return new WaitUntil(() => !pg.Is_Loading());
        save_path = pg.GetPLD().GetSavePath();
        //GetLoadingData().SaveToFile("save_test");
        while (isAutoSaving && SceneManager.GetActiveScene().name == "MainGame" && auto_save_delay >= 30) 
        {
            yield return new WaitForSecondsRealtime(auto_save_delay);

            print("QuickSave!!");
            Save();
        }
    }

    public void Save() 
    {
        var t = pg.GetPLD();
        string name = t.GetImageName().Length > 21 ? t.GetImageName()[0..20] : t.GetImageName();
        GetLoadingData().SaveToFile($"QS_{name}_{DateTime.Now:yyyy_MM_dd_HH_mm}");

        string[] fileNames = Directory.GetFiles(save_path);
        List<string> quicksaves = new List<string>();

        foreach (string fileName in fileNames)
        {
            //print(fileName[0..8]);
            if (Path.GetFileNameWithoutExtension(fileName).Length > 3 && Path.GetFileNameWithoutExtension(fileName)[0..3] == "QS_")
            {
                quicksaves.Add(fileName);

            }
        }
        if (quicksaves.Count > number_of_autosaves)
        {
            quicksaves = quicksaves.OrderByDescending(file => File.GetLastWriteTime(file)).ToList();

            for (int i = quicksaves.Count - 1; i >= number_of_autosaves; i--)
            {
                File.Delete(quicksaves[i]);
            }
        }
    }



    public void SetMaxComb(int n) 
    {
        if(n > current_max_of_combined_puzzles) 
        {
            current_max_of_combined_puzzles = n;
            if(current_max_of_combined_puzzles == number_of_puzzles_in_scene) 
            {
                StartCoroutine(GameEnd());
            }

        }
        
    }

    public PuzzleLoadingData GetLoadingData() 
    {
        return pg.GetPLD();
    }

    public void SetNumberOfPuzzles(int n) 
    {
        number_of_puzzles_in_scene = n;
    }

    private IEnumerator GameEnd() 
    {
        interaction_disabled = true;
        var sr = GetComponent<SpriteRenderer>();
        var pg = GetComponent<PuzzleGeneration>();
        var image = pg.GetImage();


        sr.sprite = Sprite.Create(image, new Rect(1, 1, image.width-1, image.height-1), new Vector2(.5f, .5f));
        transform.position = pg.GetCenter();
        sr.enabled = true;

        pg.DisableShader();

        StartCoroutine(MarkPuzzleAsComplete());

        for (float i = 0; i < 1.1f;) 
        {
            image_mat.SetFloat("_CurAnimSecond", i);
            i += 0.02f;
            yield return new WaitForFixedUpdate();
        }
        
        pg.DisablePuzzles();
        pg.EnableShader();
        interaction_disabled = false;
        print("game end");
        yield return null;
    }

    private IEnumerator MarkPuzzleAsComplete() 
    {
        yield return null;
        try 
        {
            SetPuzzleAsComplete(pg.GetPLD().GetImageNameWithExtention());
        }
        catch(IOException e) 
        {
            Debug.LogError($"Failed to create or write to the file: {e.Message}");
        }
    }

    public void SetPuzzleAsComplete(string puzzle_name)
    {

        Dictionary<string, bool> PuzzleCompletion = new Dictionary<string, bool>();
        string path = pg.GetPLD().GetJournalName();
        string jsonFile = File.ReadAllText(Path.Combine(path, $"{Path.GetFileName(path)}_completion.json"));

        DictionaryWrapper wrapper = JsonUtility.FromJson<DictionaryWrapper>(jsonFile);
        PuzzleCompletion = wrapper.ToDictionary();

        try
        {
            PuzzleCompletion[puzzle_name] = true;
            
        }
        catch (IOException e)
        {
            Debug.LogError($"Failed to find puzzle with name {puzzle_name} : {e.Message}");
        }
 
        wrapper.FromDictionary(PuzzleCompletion);
        jsonFile = JsonUtility.ToJson(wrapper);

        try
        {
            File.WriteAllText(Path.Combine(path, $"{Path.GetFileName(path)}_completion.json"), jsonFile);
        }
        catch (IOException e)
        {
            Debug.LogError($"Failed to create or write to the file: {e.Message}");
        }

    }

}
