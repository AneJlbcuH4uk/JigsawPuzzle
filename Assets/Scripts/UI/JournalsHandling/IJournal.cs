using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;


[System.Serializable]
public class DictionaryWrapper
{
    public List<string> keys = new List<string>();
    public List<bool> values = new List<bool>();

    public void FromDictionary(Dictionary<string, bool> dict)
    {
        keys.Clear();
        values.Clear();
        foreach (var kvp in dict)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }

    public Dictionary<string, bool> ToDictionary()
    {
        Dictionary<string, bool> dict = new Dictionary<string, bool>();
        for (int i = 0; i < keys.Count; i++)
        {
            dict[keys[i]] = values[i];
        }
        return dict;
    }
}

public class JournalData
{
    private List<PuzzleData> puzzles;
    private string path = "";
    private static int items_per_page = 3;
    private int number_of_pages;
    public int number_of_images_in_journal = 0;
    private string path_to_compl_file = "";

    public Dictionary<string,bool> PuzzleCompletion = new Dictionary<string, bool>();

    public string GetPath() 
    {

        //Debug.Log("given path = " + path);
        return path;

    }


    public JournalData(string path)
    {
        this.path = path;
        this.puzzles = new List<PuzzleData>();
        InitializeJournal();

    }

    private void InitializeCompletion()
    {
        path_to_compl_file = Path.Combine(Application.persistentDataPath, "PuzzleCompl", Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar)));
        string puzzle_compl_json = Path.Combine(path_to_compl_file, $"{Path.GetFileName(path)}_completion.json");

        if (File.Exists(puzzle_compl_json))
        {
            
            try
            {
                string jsonFile = File.ReadAllText(puzzle_compl_json);
                DictionaryWrapper wrapper = JsonUtility.FromJson<DictionaryWrapper>(jsonFile);
                PuzzleCompletion = wrapper.ToDictionary();

                var info = new DirectoryInfo(path);
                var fileInfo = info.GetFiles();

                foreach (var file in fileInfo)
                {
                    if (_is_file_image(file.Name))
                    {
                        number_of_images_in_journal++;
                    }
                }

                if (PuzzleCompletion.Count != number_of_images_in_journal) 
                {
                    foreach (var file in fileInfo)
                    {
                        if (_is_file_image(file.Name) && !PuzzleCompletion.ContainsKey(file.Name))
                        {
                            PuzzleCompletion.Add(file.Name, false);
                        }
                    }

                    foreach (string p in PuzzleCompletion.Keys.ToList())
                    {   
                        if (!fileInfo.Any(ele => ele.Name == p))
                        {
                            PuzzleCompletion.Remove(p);
                        }
                    }

                    wrapper.FromDictionary(PuzzleCompletion);
                    jsonFile = JsonUtility.ToJson(wrapper);

                    try
                    {
                        File.WriteAllText(puzzle_compl_json, jsonFile);
                    }
                    catch (IOException e)
                    {
                        Debug.LogError($"Failed to create or write to the file: {e.Message}");
                    }

                }


            }
            catch (IOException e)
            {
                Debug.LogError($"Failed to read the file: {e.Message}");
            }
        }
        else 
        {
            Debug.Log(path_to_compl_file);
            if (!Directory.Exists(path_to_compl_file)) 
            {
                Directory.CreateDirectory(path_to_compl_file);
                
            }

            var info = new DirectoryInfo(path);
            var fileInfo = info.GetFiles();
            foreach(var file in fileInfo) 
            {
                if (_is_file_image(file.Name)) 
                {
                    PuzzleCompletion.Add(file.Name, false);
                    number_of_images_in_journal++;
                }              
            }

            DictionaryWrapper wrapper = new DictionaryWrapper();
            wrapper.FromDictionary(PuzzleCompletion);

            string jsonFile = JsonUtility.ToJson(wrapper);

            try
            {
                File.WriteAllText(puzzle_compl_json, jsonFile);
            }
            catch (IOException e)
            {
                Debug.LogError($"Failed to create or write to the file: {e.Message}");
            }
        }
    }

    public Vector2Int GetJournalCompletionNumber() 
    {
        return new Vector2Int(PuzzleCompletion.Count(val => val.Value == true),PuzzleCompletion.Count);
    }

    public bool is_puzzle_completed(string name) 
    {
        return PuzzleCompletion[name];
    }


    private void InitializeJournal() 
    {
        InitializePuzzles();
        InitializeCompletion();
    }

    private void InitializePuzzles()
    {
        if (!Directory.Exists(path))
        {
            Debug.LogError($"Config directory does not exist: {path}");
            return;
        }

        var info = new DirectoryInfo(path);
        var fileInfo = info.GetFiles();

        foreach (var file in fileInfo)
        {
            if (_is_file_image(file.Name))
            {
                var temp = new PuzzleData(Path.Combine(path, file.Name), MaskType.Classic, 4, 5);
                puzzles.Add(temp);
            }
        }
        number_of_pages = Mathf.CeilToInt((float)puzzles.Count / items_per_page);
    }

    public static bool _is_file_image(string file_name) 
    {
        return file_name[^4..] == ".png" || file_name[^4..] == ".jpg" || file_name[^5..] == ".jpeg";
    }


    public PuzzleData[] GetPage(int page) 
    {
        PuzzleData[] res = new PuzzleData[items_per_page];

        for (int i = 0; i < items_per_page; i++)
        {
            if (i + page * items_per_page < puzzles.Count)
                res[i] = puzzles[i + page * items_per_page];
            else
                res[i] = null;
        }
        return res;
    }

    public static int GetItemsPerPage() => items_per_page;
    public int GetNumberOfPages() => number_of_pages;


    


   
}


public class PuzzleData
{
    string image;
    MaskType mt;
    int num;
    int off;

    public PuzzleData(string s, MaskType m, int n, int o)
    {
        Image = s;
        Mt = m;
        Num = n;
        Off = o;
    }

    public string Image { get => image; set => image = value; }
    public MaskType Mt { get => mt; set => mt = value; }
    public int Num { get => num; set => num = value; }
    public int Off { get => off; set => off = value; }
}
