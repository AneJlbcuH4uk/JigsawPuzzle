using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollRectDisable : MonoBehaviour
{


    [SerializeField] private int min_number_of_objects_to_enable;
    private ScrollRect scroll;
    private int number_of_objects;


    void Start()
    {
        Vector2 savedPosition = new Vector2();
        scroll = GetComponent<ScrollRect>();
        number_of_objects = transform.GetChild(0).GetChild(0).childCount;
       
        if (number_of_objects < min_number_of_objects_to_enable)
        {
            DisableScroll();
        }

        IEnumerator FixScrollPosition()
        {
            yield return new WaitForEndOfFrame(); // Wait for UI to settle
            scroll.content.anchoredPosition = savedPosition;
        }

        void DisableScroll()
        {
            savedPosition = scroll.content.anchoredPosition; // Save position
            scroll.enabled = false;
            StartCoroutine(FixScrollPosition()); // Restore position after a frame
        }
    }

}
