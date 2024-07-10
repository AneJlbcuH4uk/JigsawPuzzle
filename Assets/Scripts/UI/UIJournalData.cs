using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class UIJournalData : MonoBehaviour
{
    
    [SerializeField] private Vector2 oldUIpos;
    [SerializeField] private float oldUIRot;

    private void Awake()
    {
        oldUIpos = Vector2.zero;
    }

    public void UpdateData() 
    {
        if (oldUIpos == Vector2.zero)
        {
            oldUIpos = gameObject.GetComponent<RectTransform>().anchoredPosition;
            oldUIRot = gameObject.transform.rotation.eulerAngles.z;
            oldUIRot = Mathf.Abs(oldUIRot) > 180 ? oldUIRot = 360 - oldUIRot : oldUIRot;
        }
    }

  

    public void ClearData() 
    {
        oldUIRot = 0;
        oldUIpos = Vector2.zero;
    }

    public float GetAngle() => oldUIRot;
    public Vector2 GetPos() => oldUIpos;
}
