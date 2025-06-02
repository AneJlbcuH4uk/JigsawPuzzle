using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JournalRef : MonoBehaviour
{
    [SerializeField] private GameObject Journal;

    public void SetJournal(GameObject j) 
    {
        Journal = j;
    }

    public GameObject GetJournal() 
    {
        return Journal;
    }

    private void Start()
    {
        // Get the Toggle component
        Toggle toggle = transform.GetChild(2).GetComponent<Toggle>();
        if (toggle != null)
        { 
            // Add listener with lambda to pass extra parameter
            toggle.onValueChanged.AddListener(isOn =>
            {
                //print("onValueChanged was called");
                GameObject mainCanvas = GameObject.FindWithTag("MainCanvas");
                if (mainCanvas != null)
                {
                    UIBehaviour uiBehaviour = mainCanvas.GetComponent<UIBehaviour>();
                    if (uiBehaviour != null)
                    {
                        uiBehaviour.UpdateJournal(Journal, isOn);
                        //print("updating state");
                    }
                    else
                    {
                        Debug.LogWarning("UIBehaviour component not found on MainCanvas!");
                    }
                }
                else
                {
                    Debug.LogWarning("MainCanvas not found!");
                }
            });
        }
    }


}
