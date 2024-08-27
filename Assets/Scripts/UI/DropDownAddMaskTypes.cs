using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DropDownAddMaskTypes : MonoBehaviour
{
    private UIPuzzleData data_ref;
    private Button button_ref;
    private UISlider slider;

    void Awake()
    {
        var t = gameObject.GetComponent<TMP_Dropdown>();
        var options = Enum.GetNames(typeof(MaskType)).ToList<string>();
        t.AddOptions(options);
        t.onValueChanged.AddListener(delegate {
            UpdateData( (MaskType)t.value);
        });
        var par_obj = transform.parent.parent.parent;

        slider = transform.parent.parent.GetChild(2).GetChild(0).GetComponent<UISlider>();
        data_ref = par_obj.GetComponent<UIPuzzleData>();
        button_ref = par_obj.GetComponent<Button>();
    }

    private void UpdateData(MaskType v)
    {
        Debug.Log("Mask changed to " + v);
        data_ref.SetMaskType(v);
        slider.UpdateBoundaries();
        slider.OnSliderValueChanged();
    }

    public void SwitchStateofButton() 
    {
        button_ref.enabled = button_ref.enabled ? false: true;
    }



}
