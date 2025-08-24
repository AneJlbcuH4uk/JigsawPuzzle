using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuLoadingScreen : MonoBehaviour
{
    [SerializeField] private GameObject ProgressBar;
    [SerializeField] private GameObject LoadingScreen;

    private RectTransform ProgressBarTransform;
    [SerializeField] private int progress_bar_width;

    private void Start()
    {
        LoadingScreen.SetActive(true);
        ProgressBarTransform = ProgressBar.GetComponent<RectTransform>();
    }


    public void SetState(float state) 
    {
        progress_bar_width = -Mathf.RoundToInt(state * (-1720) + 1820);
        Vector2 offsetMax = ProgressBarTransform.offsetMax;
        offsetMax.x = progress_bar_width;
        ProgressBarTransform.offsetMax = offsetMax;
        if (state == 1)
        {
            LoadingScreen.SetActive(false);
        }
    }


}
