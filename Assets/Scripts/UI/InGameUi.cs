using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameUi : MonoBehaviour
{
    private PuzzleDataTracker dataTracker;
    [SerializeField] private GameObject UIHolder;
    [SerializeField] private GameObject LoadingScreen;
    [SerializeField] private Material mat ;
    [SerializeField] private TMP_InputField save_name_field;

    private GameObject confirm_exit_window;
    private GameObject cancel_exit_button;

    private PuzzleLoadingData PLD;

    private void Start()
    {
        cancel_exit_button = UIHolder.transform.GetChild(2).gameObject;
        confirm_exit_window = UIHolder.transform.GetChild(3).gameObject;

        PLD = dataTracker.GetLoadingData();
        if (PLD.data_was_set)
        {
            var image = PLD.GetTexture();
            var im_obj = LoadingScreen.transform.GetChild(1).GetComponent<Image>();
            im_obj.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);
            im_obj.rectTransform.sizeDelta = PLD.GetImageMeasures(); 
        }
        LoadingScreen.SetActive(true);
        mat.SetFloat("_CurFillPercent", 0.1f);
        
    }
    public void UpdateLoadState(float loadState)
    {
        mat.SetFloat("_CurFillPercent", loadState);
    }


    public IEnumerator FinishLoad() 
    {
        mat.SetFloat("_CurFillPercent", 1);
        yield return new WaitForSeconds(1);
        LoadingScreen.SetActive(false);
        GameObject.FindGameObjectWithTag("MainCanvas").SetActive(false);
    }

    private void Awake()
    {
        dataTracker = GameObject.FindGameObjectsWithTag("GameManager")[0].GetComponent<PuzzleDataTracker>();
    }

    public void LeaveButton() 
    {
        dataTracker.Save();
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public void Pause()
    {
        UIHolder.SetActive(!UIHolder.activeSelf);
        dataTracker.SetInteractionBool(UIHolder.activeSelf);
        cancel_exit_button.SetActive(false);
        confirm_exit_window.SetActive(false);
    }

    public bool IsUIActive() => UIHolder.activeSelf;

    public void OnSaveMenuOpen() 
    {
        string name = PLD.GetImageName().Length > 21 ? PLD.GetImageName()[0..20] : PLD.GetImageName();
        save_name_field.text = $"QS_{name}_{DateTime.Now:yyyy_MM_dd_HH_mm}";
    }

    public void OnSaveFieldClick() 
    {
        save_name_field.text = "";
    }

    public void OnSaveButtonClick() 
    {
        if(save_name_field.text == "" || save_name_field.text[0..3] == "QS_") 
        {
            dataTracker.Save();
        }
        else 
        {
            PLD.SaveToFile(save_name_field.text);
        }
    }


}
