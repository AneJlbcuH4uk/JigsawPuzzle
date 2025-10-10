using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;


public class ChangeGeneralSettings : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown LanguageDrop;
    [SerializeField] private Toggle SystemClockToggle;
    [SerializeField] private SettingsInit config;
    [SerializeField] private GameObject Confirmation_screen;
    [SerializeField] private GameObject CGS_Canvas;
    [SerializeField] private UIBehaviour UIB;
    [SerializeField] private Toggle AutosaveToggle;
    [SerializeField] private Slider AutosaveFrequencySlider;
    [SerializeField] private Slider AutosaveNumberSlider;
    [SerializeField] private GameObject SystemClock;
    

    [SerializeField] private Vector2 FrequencyRange = new Vector2(1, 60);

    private GeneralSettings generalSettings;
    [SerializeField] private bool SettingsWasChanged = false;


    [SerializeField] private AudioClip toggleClick;
    private bool is_initializing = false;

    private void Awake()
    {
        is_initializing = true;
        generalSettings = config.ReloadGeneralSettings();
        FontManager.GetInstance().ChangeFont(generalSettings.language);

        UIB = GameObject.FindGameObjectWithTag("MainCanvas").GetComponent<UIBehaviour>();

        List<string> options = new List<string>();
        foreach (var loc in LocalizationSettings.AvailableLocales.Locales) 
        {
            options.Add(loc.Identifier.ToString());
        }
        LanguageDrop.AddOptions(options);
        Open();
        is_initializing = false;
    }

    public void Open() 
    {
        RestoreSettings();
        UIB.RefreshAnimationCounter();
        
    }




    public void OnNumberOfSavesSliderChange() 
    {
        generalSettings.number_of_auto_saves = (int)AutosaveNumberSlider.value;
        SettingsWasChanged = true;
    }

    public void OnButtonSave() 
    {
        if (!SettingsWasChanged) return;

        print(generalSettings);
        config.SaveSettings(generalSettings, Settings.General);
        SettingsWasChanged = false;
        SystemClock.SetActive(generalSettings.systemClock);
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[LanguageDrop.value];
        FontManager.GetInstance().ChangeFont(generalSettings.language);
    }


    public void OnLangageChanged() 
    {
        int l = LanguageDrop.captionText.text.Length;
        generalSettings.language = LanguageDrop.captionText.text.Substring(l-3,2);
        LanguageDrop.Hide();
        SettingsWasChanged = true;
    }

    private int GetLocaleIndexByCode(string languageCode)
    {
        int res = 0;
        // Get the list of available locales
        List<Locale> availableLocales = LocalizationSettings.AvailableLocales.Locales;

        // Find the locale that matches the provided language code
        foreach (Locale locale in availableLocales)
        {
            if (locale.Identifier.Code == languageCode)
            {
                return res;
            }
            res += 1;
        }

        Debug.LogWarning("Locale with code '" + languageCode + "' not found.");
        return 0;
    }

    public void OnToggleChangeSystemClock() 
    {
        if (!is_initializing)
        {
            SoundFXManager.instance.PlaySoundClipOnClick(toggleClick);
        }

        generalSettings.systemClock = SystemClockToggle.isOn;
        SettingsWasChanged = true;
    }

    public void OnToggleChangeAutosave()
    {
        if (!is_initializing) 
        {
            SoundFXManager.instance.PlaySoundClipOnClick(toggleClick);
        }

        generalSettings.autosavetoggle = AutosaveToggle.isOn;
        AutosaveFrequencySlider.gameObject.SetActive(generalSettings.autosavetoggle);
        AutosaveNumberSlider.gameObject.SetActive(generalSettings.autosavetoggle);
        SettingsWasChanged = true;
    }

    public void OnFrequencySliderChange() 
    {
        generalSettings.autosaveFrequency = AutosaveFrequencySlider.value;
        SettingsWasChanged = true;
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

    public void CancelChanges() 
    {
        RestoreSettings();
    }

    public void ChangeNoSaveInMenu()
    {
        UIB.unsaved_in_menu = true;
        //RestoreSettings();
    }

    

    public void RestoreSettings() 
    {
        if (config == null)
        {
            config = GameObject.FindWithTag("MainCanvas").GetComponent<SettingsInit>();
        }


        generalSettings = config.ReloadGeneralSettings();
        int locale_index = GetLocaleIndexByCode(generalSettings.language);

        LanguageDrop.value = locale_index;
        SystemClockToggle.isOn = generalSettings.systemClock;
        AutosaveToggle.isOn = generalSettings.autosavetoggle;
        AutosaveFrequencySlider.gameObject.SetActive(generalSettings.autosavetoggle);
        AutosaveFrequencySlider.maxValue = FrequencyRange.y;
        AutosaveFrequencySlider.minValue = FrequencyRange.x;
        AutosaveFrequencySlider.value = generalSettings.autosaveFrequency;

        AutosaveNumberSlider.value = generalSettings.number_of_auto_saves;

        SettingsWasChanged = false;
    }

}
