using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowSliderValue : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] float multiplier = 1;

    [SerializeField] string suffix = "";

    private TMP_Text number;

    private void Awake()
    {
        number = gameObject.GetComponent<TMP_Text>();
    }


    void Update()
    {
        number.text = "" + (slider.value * multiplier) + suffix;
    }
}
