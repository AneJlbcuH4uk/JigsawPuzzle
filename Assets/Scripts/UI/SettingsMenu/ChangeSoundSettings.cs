using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.Audio;

public class ChangeSoundSettings : MonoBehaviour
{
    [SerializeField] private Slider master;
    [SerializeField] private Slider music;
    [SerializeField] private Slider effects;
    [SerializeField] private Toggle mute;
    [SerializeField] private Toggle mute_on_min;
    [SerializeField] private GameObject CSS_Canvas;
    [SerializeField] private GameObject Confirmation_screen;
    [SerializeField] private UIBehaviour UIB;
    [SerializeField] private AudioMixer mixer;

    [SerializeField] private SettingsInit config;

    private SoundSettings soundSettings;
    [SerializeField] private bool SettingsWasChanged = false;

    [SerializeField] private AudioClip toggleClick;
    private bool is_initializing = false;


    private void Awake()
    {
        is_initializing = true;
        UIB = GameObject.FindGameObjectWithTag("MainCanvas").GetComponent<UIBehaviour>();
  
        Open();
        is_initializing = false;
    }

    public void Open() 
    {
        RestoreSettings();

        UpdateMixerVolume();

        UIB.RefreshAnimationCounter();

        //print(SettingsWasChanged);
    }

    public void RestoreSettings() 
    {
        if (config == null)
        {
            config = GameObject.FindWithTag("MainCanvas").GetComponent<SettingsInit>();
            //print(config + "  WHY???");
        }

        soundSettings = config.ReloadSoundSettings();

        master.value = soundSettings.GeneralSound;
        music.value = soundSettings.MusicSound;
        effects.value = soundSettings.EffectsSound;
        mute.isOn = soundSettings.MuteApp;
        mute_on_min.isOn = soundSettings.MuteOnMin;

        SettingsWasChanged = false;

    }


    public void UpdateMixerVolume() 
    {
        mixer.SetFloat("MasterVolume", UnNormilizeSoundValue(soundSettings.GeneralSound));
        mixer.SetFloat("MusicVolume", UnNormilizeSoundValue(soundSettings.MusicSound));
        mixer.SetFloat("EffectsVolume", UnNormilizeSoundValue(soundSettings.EffectsSound));
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

    public void OnMasterSliderChange() 
    {
        soundSettings.GeneralSound = master.value;
        mixer.SetFloat("MasterVolume", UnNormilizeSoundValue(master.value));
        SettingsWasChanged = true;
    }
    public void OnMusicSliderChange()
    {
        soundSettings.MusicSound = music.value;
        mixer.SetFloat("MusicVolume", UnNormilizeSoundValue(music.value));
        SettingsWasChanged = true;
    }
    public void OnEffectsSliderChange()
    {
        soundSettings.EffectsSound = effects.value;
        mixer.SetFloat("EffectsVolume", UnNormilizeSoundValue(effects.value));
        SettingsWasChanged = true;
    }
    public void OnMuteToggleChange()
    {
        if (!is_initializing)
        {
            SoundFXManager.instance.PlaySoundClipOnClick(toggleClick);
        }

        AudioListener.pause = mute.isOn;
        soundSettings.MuteApp = mute.isOn;
        SettingsWasChanged = true;
    }
    public void OnMuteonMinChange()
    {
        if (!is_initializing)
        {
            SoundFXManager.instance.PlaySoundClipOnClick(toggleClick);
        }
        Application.runInBackground = !mute_on_min.isOn;
        soundSettings.MuteOnMin = mute_on_min.isOn;
        SettingsWasChanged = true;
    }

    public void OnButtonSave()
    {
        print("Sound" + SettingsWasChanged);
        if (!SettingsWasChanged) return;
        // change sound


        config.SaveSettings(soundSettings, Settings.Sound);
        SettingsWasChanged = false;
    }

    public void CancelChanges()
    {
        RestoreSettings();
        UpdateMixerVolume();
    }

    public void ChangeNoSaveInMenu()
    {
        UIB.unsaved_in_menu = true;
    }

    public static float UnNormilizeSoundValue(float val) 
    {
        if (val < 0.0001) val = 0.0001f;
        return Mathf.Log10(val) * 20f;
    }

}
