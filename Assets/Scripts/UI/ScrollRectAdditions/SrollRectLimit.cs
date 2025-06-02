using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SrollRectLimit : MonoBehaviour
{

    [SerializeField] RectMask2D mask;
    [SerializeField] private int possible_offset = 0;
    [SerializeField] int shift = 520;
    [SerializeField] int empty_space_top = 300;

    private void Start()
    {
        ScrolLRectLimit();
    }

    public void ScrolLRectLimit()
    {
        var Rect = this.GetComponent<RectTransform>();

        possible_offset = (int)(Rect.sizeDelta.y - shift);

        bool reached_top = Rect.anchoredPosition.y < -empty_space_top;
        bool reached_bot = Rect.anchoredPosition.y + empty_space_top > possible_offset;

        if (reached_top) 
        {
            Rect.anchoredPosition = new Vector2(Rect.anchoredPosition.x, -empty_space_top);
        }
        if (reached_bot)
        {
            Rect.anchoredPosition = new Vector2(Rect.anchoredPosition.x, possible_offset - empty_space_top);
        }
        if(!(reached_bot || reached_top))
        {
            Vector4 padding = mask.padding;
            padding.x = 0;
            padding.w = Rect.anchoredPosition.y + empty_space_top;
            padding.z = 0;
            padding.y = Rect.sizeDelta.y - shift - (Rect.anchoredPosition.y + empty_space_top);

            mask.padding = padding;
            mask.enabled = false;
            mask.enabled = true;
        }

    }

}
