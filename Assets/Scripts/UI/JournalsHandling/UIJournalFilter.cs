using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class UIJournalFilter : MonoBehaviour
{

    private string path_to_set_folders = Application.streamingAssetsPath + "/Puzzles"; // probably better use Recources 
    private string path_to_filter_config = Application.streamingAssetsPath + "/Config/filter_config.json";

    List<string> journals;

    [SerializeField] private GameObject UIprefab;
    [SerializeField] private GameObject collections_holder;
    
    private Transform holder_content;
    private Dictionary<string, bool> journalStates = new Dictionary<string, bool>();

    //private void Awake()
    //{
    //    journals = Directory.GetDirectories(path_to_set_folders).ToList<string>();
    //    holder_content = collections_holder.transform.GetChild(0).GetChild(0);

    //    var startpos = new Vector3(0, journals.Count * 30 - 30, 0);
    //    int index = 0;
    //    foreach (var j in journals)
    //    {
    //        var delta = collections_holder.GetComponent<RectTransform>().sizeDelta;

    //        collections_holder.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 60) + delta;

    //        var t = Instantiate(UIprefab, holder_content);
    //        t.GetComponent<RectTransform>().anchoredPosition = startpos;
    //        var text = t.transform.GetChild(1).GetComponent<TMP_Text>();
    //        text.text = Path.GetFileNameWithoutExtension(j);

    //        SetJournalState(index, true);
    //        index++;
    //        startpos.y -= 60;
    //    }
    //}






    private void Awake()
    {
        journals = Directory.GetDirectories(path_to_set_folders).ToList();
        //print(journals[0]);

        holder_content = collections_holder.transform.GetChild(0).GetChild(0);

        LoadFilter(); // Load previous states

        var startpos = new Vector3(0, journals.Count * 30 - 30, 0);
        int index = 0;
        foreach (var j in journals)
        {
            var delta = collections_holder.GetComponent<RectTransform>().sizeDelta;
            collections_holder.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 60) + delta;

            var t = Instantiate(UIprefab, holder_content);
            t.GetComponent<RectTransform>().anchoredPosition = startpos;

            var text = t.transform.GetChild(1).GetComponent<TMP_Text>();
            string journalName = Path.GetFileNameWithoutExtension(j);
            text.text = journalName;

            // Get toggle component and apply saved state

            if (journalStates.ContainsKey(journalName))
            {
                SetJournalState(index, journalStates[journalName]);
            }
            else
            {
                journalStates[journalName] = true;
                SetJournalState(index, true); // Default to true if no saved state
            }
            index++;
            startpos.y -= 60;

            
        }
        //StartCoroutine(ApplyLoadedFilter());

    }

    public IEnumerator ApplyLoadedFilter() 
    {
        yield return new WaitForEndOfFrame();
        int index = 0;

        foreach (var j in journals)
        {
            string journalName = Path.GetFileNameWithoutExtension(j);

            // Get toggle component and apply saved state

            if (journalStates.ContainsKey(journalName))
            {
                SetJournalState(index, journalStates[journalName]);
            }
            else
            {
                journalStates[journalName] = true;
                SetJournalState(index,true); // Default to true if no saved state
            }
            index++;
        }
    }



    private void SaveFilter()
    {
        // Collect toggle states
        foreach (Transform child in holder_content)
        {
            string journalName = child.GetChild(1).GetComponent<TMP_Text>().text;
            Toggle toggle = child.GetComponentInChildren<Toggle>();
            journalStates[journalName] = toggle.isOn;
        }

        // Convert to DictionaryWrapper and save as JSON
        DictionaryWrapper wrapper = new DictionaryWrapper();
        wrapper.FromDictionary(journalStates);
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(path_to_filter_config, json);
        //Debug.Log($"Filter saved: {json}");
    }

    private void LoadFilter()
    {
        if (File.Exists(path_to_filter_config))
        {
            string json = File.ReadAllText(path_to_filter_config);
            DictionaryWrapper wrapper = JsonUtility.FromJson<DictionaryWrapper>(json);
            journalStates = wrapper.ToDictionary();
            //Debug.Log($"Filter loaded: {json}");
        }
        else
        {
            Debug.LogWarning("No filter config found, using defaults.");
        }
    }


    public void SelectAll() 
    {
        for (int i = 0; i < holder_content.childCount; i++)
        {
            SetJournalState(i, true);
        }
    }

    public void InverSelect()
    {
        for (int i = 0; i < holder_content.childCount; i++)
        {
            var b = GetJournalState(i);
            SetJournalState(i, !b);
        }
    }

    public void HideCompleted() 
    {
        for (int i = 0; i < holder_content.childCount; i++)
        {
            
            string puzzle_compl_json = Path.Combine(journals[i], $"{Path.GetFileName(journals[i])}_completion.json");
            if (File.Exists(puzzle_compl_json))
            {
                try
                {
                    string jsonFile = File.ReadAllText(puzzle_compl_json);
                    if (jsonFile.Contains("false")) 
                    {
                        continue;
                    }
                }
                catch (IOException e)
                {
                    Debug.LogError($"Failed to read the file: {e.Message}");
                }
            }
            else
            {
                continue;
            }

            SetJournalState(i, false);


        }
    }

    private void SetJournalState(int index ,bool state) 
    {
        holder_content.GetChild(index).GetChild(2).GetComponent<Toggle>().isOn = state;
    }

    private IEnumerator SetJournalState2(int index, bool state)
    {
        yield return new WaitForEndOfFrame();
        //print($"changing state of {index} element to {state}");
        holder_content.GetChild(index).GetChild(2).GetComponent<Toggle>().isOn = state;
    }

    public bool GetJournalState(int index) 
    {
        return holder_content.GetChild(index).GetChild(2).GetComponent<Toggle>().isOn;
    }

    public void SetRef(GameObject g, int index) 
    {
        holder_content.GetChild(index).GetComponent<JournalRef>().SetJournal(g);    
    }

    public List<string> GetPuzzleSets() 
    {
        //List<string> active_journals = new List<string>();

        //for (int i = 0; i < holder_content.childCount; i++)
        //{
        //    //if (GetJournalState(i)) 
        //    //{
        //    print(journals[i]);
        //    //}
        //}

        return journals;
    }


    [System.Serializable]
    private class ToggleSaveData
    {
        public Dictionary<string, bool> states;

        public ToggleSaveData(Dictionary<string, bool> states)
        {
            this.states = states;
        }
    }
}


