using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject savefile_ui_prefab;
    [SerializeField] private List<string> save_files;
    [SerializeField] private GameObject old_saves_holder;
    [SerializeField] private GameObject last_save_holder;
    [SerializeField] private RectMask2D mask;
    [SerializeField] private ScrollRect scroll;

    [SerializeField] private GameObject SelectButton;
    [SerializeField] private GameObject SelectAllButton;
    [SerializeField] private GameObject DeleteButton;
    [SerializeField] private GameObject RenameButton;
    [SerializeField] private GameObject NoSaveFilesImage;

    [SerializeField] private TMP_InputField rename_field;

    private Transform saveholder;
    private string path_to_saves;

    private void Awake()
    {
        saveholder = old_saves_holder.transform.GetChild(0).GetChild(0);
        path_to_saves = Path.Combine(Application.persistentDataPath, "Saves");
        ResetMenu();
    }

    private void updatemask(float val) 
    {
        Vector4 padding = mask.padding;
        padding.x = 0;
        padding.w = 0;
        padding.z = 0;
        padding.y = val;

        mask.padding = padding;
        mask.enabled = false;
        mask.enabled = true;
    }

    private bool is_selecting = false;
    [SerializeField] private List<UISaveData> selected_saves = new List<UISaveData>();

    public void OnButtonSelect() 
    {
        is_selecting = !is_selecting;
        SelectAllButton.SetActive(is_selecting);
        SlideOldSaves(new Vector3(is_selecting ? -15: 15, 0));
        if(is_selecting == false) 
        {
            ClearSelectedSaves();
        }

    }

    public void OnButtonRename() 
    {
        if (rename_field.text == "" || rename_field.text == selected_saves[0].GetName()) return;
        else 
        {
            selected_saves[0].RenameFile(rename_field.text);
        }

    }


    public void OnButtonOpenRename()
    {
        rename_field.text = selected_saves[0].GetName();
    }

    public void ClearField() 
    {
        rename_field.text = "";
    }


    private void SlideOldSaves(Vector3 slide) 
    {
        var save = last_save_holder.transform.GetChild(1);
        set();

        for (int i = 0; i < saveholder.childCount; i++)
        {
            save = saveholder.GetChild(i);
            set();
        }

        void set()
        {
            save.transform.position += slide;
            save.transform.GetChild(4).gameObject.SetActive(is_selecting);
        }
    }

    public void AddToSelectedSaves(UISaveData obj) 
    {
        DeleteButton.SetActive(true);       
        selected_saves.Add(obj);
        if (selected_saves.Count == 1)
        {
            RenameButton.SetActive(true);
        }
        else
        {
            RenameButton.SetActive(false);
        }
        is_all_saves_selected = selected_saves.Count >= save_files.Count; // -1 temp
    }

    public void RemoveFromSelectedSaves(UISaveData obj) 
    {
        is_all_saves_selected = false;
        selected_saves.Remove(obj);
        if (selected_saves.Count == 1)
        {
            RenameButton.SetActive(true);
        }
        else
        {
            RenameButton.SetActive(false);
        }
        if (selected_saves.Count == 0) 
        {
            DeleteButton.SetActive(false);
        }
    }

    private void ClearSelectedSaves()
    {
        is_all_saves_selected = false;
        DeleteButton.SetActive(false);
        RenameButton.SetActive(false);
        var save = last_save_holder.transform.GetChild(1);
        reset();

        for (int i = 0; i < saveholder.childCount; i++)
        {
            save = saveholder.GetChild(i);
            reset();
        }    
        
        void reset() 
        {
            save.GetComponent<UISaveData>().ResetToggle();
        }


        selected_saves.Clear();
    }

    public void ResetState()
    {
        if (is_selecting)
        {
            OnButtonSelect();
        }
    }
    bool is_all_saves_selected = false;
    public void OnSelectAllButton() 
    {
        if (!is_all_saves_selected)
        {
            
            for (int i = 0; i < saveholder.childCount; i++)
            {
               
                var save = saveholder.GetChild(i).GetComponent<UISaveData>();
                save.SetToggle(true);
            }
        }
        else
        {
            ClearSelectedSaves();
        }
    }

    public void OnButtonDelete() 
    {
        for(int i = 0; i < selected_saves.Count; i++) 
        {
            selected_saves[i].DeleteSave();
        }
        ResetState();
        ResetMenu();
        
    } 

    private void ResetMenu() 
    {
        int lastsave_index = 0;
        old_saves_holder.GetComponent<RectTransform>().sizeDelta = new Vector2(588, 40);

        if(last_save_holder.transform.childCount > 1) 
        {
            Destroy(last_save_holder.transform.GetChild(1).gameObject);
        }


        if(saveholder.childCount > 0) 
        {
            for(int i = 0; i < saveholder.childCount; i++) 
            {
                Destroy(saveholder.GetChild(i).gameObject);
            }
        }

        if (Directory.Exists(path_to_saves))
        {
            save_files = Directory.GetFiles(path_to_saves).ToList<string>();
        }
        else 
        {
            Directory.CreateDirectory(path_to_saves);
        }


        if (save_files.Count != 0)
        {
            old_saves_holder.SetActive(true);
            last_save_holder.transform.GetChild(0).gameObject.SetActive(true);
            NoSaveFilesImage.SetActive(false);
            SelectButton.SetActive(true);

            DateTime last_write_time = File.GetLastWriteTime(Path.Combine(path_to_saves, save_files[0]));
            save_files = save_files.OrderByDescending(file => File.GetLastWriteTime(file)).ToList<string>();

            var last_save_pos = new Vector3(0, -55, transform.position.z);
            var worldPosition = last_save_holder.transform.TransformPoint(last_save_pos);
            var last_save = Instantiate(savefile_ui_prefab, worldPosition, Quaternion.identity, last_save_holder.transform);
            var data = last_save.GetComponent<UISaveData>();

            StartCoroutine(data.SetUISaveData(save_files[lastsave_index]));
            save_files.RemoveAt(lastsave_index);

            if (save_files.Count == 0)
            {
                old_saves_holder.SetActive(false);
                return;
            }

            var old_save_pos = new Vector3(0, save_files.Count * 80 - 80, 0);
            
            foreach (var save in save_files)
            {
                var delta = old_saves_holder.GetComponent<RectTransform>().sizeDelta;
                
                old_saves_holder.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 160) + delta;
                var t = Instantiate(savefile_ui_prefab, old_saves_holder.transform.GetChild(0).GetChild(0));
                t.GetComponent<RectTransform>().anchoredPosition = old_save_pos;

                data = t.GetComponent<UISaveData>();
                StartCoroutine(data.SetUISaveData(save));

                old_save_pos.y -= 160;
            }

            updatemask(old_saves_holder.GetComponent<RectTransform>().sizeDelta.y - 520);

        }
        else 
        {
            old_saves_holder.SetActive(false);
            last_save_holder.transform.GetChild(0).gameObject.SetActive(false);
            SelectButton.SetActive(false);
            NoSaveFilesImage.SetActive(true);
        }

    }

    


}
