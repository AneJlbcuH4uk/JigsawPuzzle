using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class ChangeSoundSettings : MonoBehaviour
{
    [SerializeField] private Slider master;
    [SerializeField] private Slider music;
    [SerializeField] private Slider effects;
    [SerializeField] private Toggle mute;
    [SerializeField] private Toggle mute_on_min;

    [SerializeField] private SettingsInit config;

    private SoundSettings soundSettings;


    private void Start()
    {
        soundSettings = config.GetSoundSettings();

        master.value = soundSettings.GeneralSound;
        music.value = soundSettings.MusicSound;
        effects.value = soundSettings.EffectsSound;
        mute.isOn = soundSettings.MuteApp;
        mute_on_min.isOn = soundSettings.MuteOnMin;
    }

    public void OnMasterSliderChange() 
    {
        soundSettings.GeneralSound = master.value;
    }
    public void OnMusicSliderChange()
    {
        soundSettings.MusicSound = music.value;
    }
    public void OnEffectsSliderChange()
    {
        soundSettings.EffectsSound = effects.value;
    }
    public void OnMuteToggleChange()
    {
        soundSettings.MuteApp = mute.isOn;
    }
    public void OnMuteonMinChange()
    {
        soundSettings.MuteOnMin = mute_on_min.isOn;
    }

    public void OnButtonSave()
    {
        // change sound


        config.SaveSettings(soundSettings, Settings.Sound);      
    }

}
