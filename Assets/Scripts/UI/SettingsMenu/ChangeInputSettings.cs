using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeInputSettings : MonoBehaviour
{
    [SerializeField] private GameObject Confirmation_screen;
    [SerializeField] private GameObject CIS_Canvas;
    [SerializeField] private UIBehaviour UIB;

    [SerializeField] private SettingsInit config;
    private bool SettingsWasChanged = false;

    private InputSettings InputSettings;

    private void Start()
    {
        UIB = GameObject.FindGameObjectWithTag("MainCanvas").GetComponent<UIBehaviour>();
    }


    public void Open()
    {
        //InputSettings = config.ReloadInputSettings();
        SettingsWasChanged = false;
        UIB.RefreshAnimationCounter();
    }


    public void OnButtonSave()
    {
        if (!SettingsWasChanged) return;

        config.SaveSettings(InputSettings, Settings.Input);
        SettingsWasChanged = false;
    }

    public bool IsSettingsWasChanged() => SettingsWasChanged;


    public void OnExitWithoutConfirmation() 
    {
        if (SettingsWasChanged)
        {
            Confirmation_screen.SetActive(true);
        }
        else 
        {
            UIB.SubtractAnimationCounter();
        }
    }
    public void CancelChanges()
    {
        SettingsWasChanged = false;
    }

    public void ChangeNoSaveInMenu() 
    {        
        UIB.unsaved_in_menu = true;   
    }

}
