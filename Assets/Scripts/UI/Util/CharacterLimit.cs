using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterLimit : MonoBehaviour
{
    private TMP_InputField field;
    public int character_limit = 20;
    private void Awake()
    {
        field = gameObject.GetComponent<TMP_InputField>();
    }
    public void OnCharacterEnter() 
    {
        field = gameObject.GetComponent<TMP_InputField>();
        if (field.text.Length > character_limit) 
        {
            field.text = field.text[0..character_limit];
        }
    }
}
