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

    private GeneralSettings generalSettings;

    private void Start()
    {
        generalSettings = config.GetGeneralSettings();
        
        List<string> options = new List<string>();

        foreach (var loc in LocalizationSettings.AvailableLocales.Locales) 
        {
            options.Add(loc.Identifier.ToString());
            print(loc.Identifier.Code);
        }
        LanguageDrop.AddOptions(options);
        int locale_index = GetLocaleIndexByCode(generalSettings.language);

        LanguageDrop.value = locale_index;
        SystemClockToggle.isOn = generalSettings.systemClock;
    }

    public void OnButtonSave() 
    {
        config.SaveSettings(generalSettings, Settings.General);
        //----------- add ------------------- set clock on
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[LanguageDrop.value];
    }


    public void OnLangageChanged() 
    {
        int l = LanguageDrop.captionText.text.Length;
        generalSettings.language = LanguageDrop.captionText.text.Substring(l-3,2);
        LanguageDrop.Hide();
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
        generalSettings.systemClock = SystemClockToggle.isOn;
    }


}
