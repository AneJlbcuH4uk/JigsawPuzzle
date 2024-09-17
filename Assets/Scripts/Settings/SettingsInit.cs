using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

public enum Settings 
{
    General,
    Graphics,
    Sound,
    Input
}

public class SettingsInit : MonoBehaviour
{
    public string path_to_graphics_settings;
    public string path_to_sound_settings;
    public string path_to_general_settings;
    public string path_to_input_settings;

    private GraphicSettings _graphicSettings;
    private SoundSettings _soundSettings;
    private InputSettings _inputSettings;
    private GeneralSettings _generalSettings;

    private string[] settings_type = new string[] { "GeneralSettings.json","GraphicsSettings.json", "SoundSettings.json", "InputSettings.json",  };


    void Start()
    {
        path_to_graphics_settings = GetPath(Settings.Graphics);
        path_to_sound_settings = GetPath(Settings.Sound);
        path_to_general_settings = GetPath(Settings.General);
        path_to_input_settings = GetPath(Settings.Input);

        // Call the method to set or load settings.
        LoadSettings(ref _graphicSettings, path_to_graphics_settings, (graphicSettings) =>
        {
            graphicSettings.CreateNew((int)FullScreenMode.ExclusiveFullScreen, new Vector2Int(Screen.mainWindowDisplayInfo.width, Screen.mainWindowDisplayInfo.height));
        });

        LoadSettings(ref _soundSettings, path_to_sound_settings, (SoundSettings) =>
        {
            SoundSettings.CreateNew(0.5f,0.5f,0.5f,false,false);
        });
        
        LoadSettings(ref _inputSettings, path_to_input_settings, (inputSettings) =>
        {
            inputSettings.CreateNew();
        });
        
        LoadSettings(ref _generalSettings, path_to_general_settings, (generalSettings) =>
        {
            generalSettings.CreateNew("en",false);
        });

        SetLoaded();

    }

    private void SetLoaded() 
    {
        //graphics
        Screen.fullScreenMode = (FullScreenMode)_graphicSettings.FullscreenMode;
        Screen.SetResolution(_graphicSettings.ScreenResolutionWidth, _graphicSettings.ScreenResolutionHeight, Screen.fullScreenMode);

        //general
        StartCoroutine(WaitForLocalizationInitialization());

    }

    private IEnumerator WaitForLocalizationInitialization()
    {
        // Wait for the initialization operation to complete
        AsyncOperationHandle initializationOperation = LocalizationSettings.InitializationOperation;
        yield return initializationOperation;

        if (initializationOperation.Status == AsyncOperationStatus.Succeeded)
        {
            // Once initialization is complete, set the locale by code
            SetLocaleByCode(_generalSettings.language);
        }
        else
        {
            Debug.LogError("Localization initialization failed: " + initializationOperation.OperationException);
        }
    }


    public void SetLocaleByCode(string languageCode)
    {
        // Get the list of available locales
        List<Locale> availableLocales = LocalizationSettings.AvailableLocales.Locales;

        // Find the locale that matches the provided language code
        foreach (Locale locale in availableLocales)
        {
            if (locale.Identifier.Code == languageCode)
            {
                // Set the selected locale
                LocalizationSettings.SelectedLocale = locale;
                Debug.Log("Locale set to: " + locale.LocaleName);
                return;
            }
        }

        Debug.LogWarning("Locale with code '" + languageCode + "' not found.");
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
    }



    private void LoadSettings<T>(ref T settingsInstance, string path, Action<T> createNewInstance) where T : class, new()
    {
        // Check if the directory exists.
        string directoryPath = Path.GetDirectoryName(path);

        if (!Directory.Exists(directoryPath))
        {
            Debug.LogError($"Config directory does not exist: {directoryPath}");
            return;
        }

        // Check if the settings file exists.
        if (File.Exists(path))
        {
            // Read settings from the file.
            try
            {
                LoadSettingsType(ref settingsInstance, path);
            }
            catch (IOException e)
            {
                Debug.LogError($"Failed to read the settings file: {e.Message}");
            }
        }
        else
        {
            // Create new settings and serialize to JSON.
            settingsInstance = new T();
            createNewInstance(settingsInstance);  // Call the specific creation logic
            string jsonSettings = JsonUtility.ToJson(settingsInstance);

            try
            {
                // Write the settings to the file.
                File.WriteAllText(path, jsonSettings);
                Debug.Log($"{typeof(T).Name} settings file created and written successfully.");
            }
            catch (IOException e)
            {
                Debug.LogError($"Failed to create or write to the settings file: {e.Message}");
            }
        }



    }

    private void LoadSettingsType<T>(ref T settingsInstance, string path) where T : class
    {
        string set = File.ReadAllText(path);
        settingsInstance = JsonUtility.FromJson<T>(set);
        Debug.Log($"{typeof(T).Name} loaded successfully.");
    }

    public GraphicSettings GetGraphicsSettings() => _graphicSettings;
    public SoundSettings GetSoundSettings() => _soundSettings;
    public GeneralSettings GetGeneralSettings() => _generalSettings;
    public InputSettings GetInputSettings() => _inputSettings;

    public void SaveSettings<T>(T settingsInstance, Settings type) where T : class
    {
        string jsonSettings = JsonUtility.ToJson(settingsInstance);

        try
        {
            // Write the settings to the file.
            File.WriteAllText(GetPath(type), jsonSettings);
            Debug.Log($"{typeof(T).Name} settings file created and written successfully.");
        }
        catch (IOException e)
        {
            Debug.LogError($"Failed to create or write to the settings file: {e.Message}");
        }
    }


    private string GetPath(Settings type)  => Path.Combine(Application.streamingAssetsPath, "Config/", settings_type[(int) type]);



}
