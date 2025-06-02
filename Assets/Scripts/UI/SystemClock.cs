using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SystemClock : MonoBehaviour
{
    private TMP_Text timeText;
    private GeneralSettings _GeneralSettings;

    private void Start()
    {
        timeText = gameObject.GetComponent<TMP_Text>();
        var _Main_canvas = GameObject.FindGameObjectWithTag("MainCanvas");
        
        _GeneralSettings = _Main_canvas.GetComponent<SettingsInit>().ReloadGeneralSettings();
        gameObject.SetActive(_GeneralSettings.systemClock);

        UpdateTimeText();
        if(_GeneralSettings.systemClock)
        StartCoroutine(UpdateTimeOnMinute());
    }

    private IEnumerator UpdateTimeOnMinute()
    {
        while (true)
        {
            UpdateTimeText();

            // Calculate time until the next full minute
            DateTime now = DateTime.Now;
            int secondsUntilNextMinute = 60 - now.Second;

            yield return new WaitForSeconds(secondsUntilNextMinute); // Wait for the next full minute
        }
    }

    private void UpdateTimeText()
    {
        timeText.text = DateTime.Now.ToString("HH:mm");
    }
}
