using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class UISlider : MonoBehaviour
{
    private UIPuzzleData data_ref;
    private Slider slider;

    private TMP_InputField x_field;
    private TMP_InputField y_field;

    void Awake()
    {
        x_field = transform.parent.GetChild(2).GetComponent<TMP_InputField>();
        y_field = transform.parent.GetChild(3).GetComponent<TMP_InputField>();

        x_field.readOnly = true;
        y_field.readOnly = true;


        var par_obj = transform.parent.parent.parent;
        data_ref = par_obj.GetComponent<UIPuzzleData>();
        slider = GetComponent<Slider>();
        slider.wholeNumbers = true;
        UpdateBoundaries();
        OnSliderValueChanged();
        slider.onValueChanged.AddListener(delegate {
            OnSliderValueChanged();
        });

    }

    public void OnSliderValueChanged() 
    {
        y_field.text = slider.value.ToString();
        x_field.text = data_ref.GetAmountOfPuzzlesInWidth((int)slider.value).ToString();
        data_ref.SetNumberOfPuzzles((int)slider.value);
    }


    public void UpdateBoundaries() 
    {
        slider.minValue = 2;
        slider.maxValue = data_ref.GetMaxAmountOfPuzzles()[1];
    }

}
