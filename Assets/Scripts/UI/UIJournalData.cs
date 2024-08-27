using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class UIJournalData : MonoBehaviour
{
    
    [SerializeField] private Vector2 oldUIpos;
    [SerializeField] private float oldUIRot;
    [SerializeField] private JournalData puzzle_images_path;

    private void Start()
    {
        oldUIpos = Vector2.zero;
    }

    public void UpdateData() 
    {
        if (oldUIpos == Vector2.zero)
        {
            oldUIpos = gameObject.GetComponent<RectTransform>().anchoredPosition;
            oldUIRot = gameObject.transform.rotation.eulerAngles.z;
            oldUIRot = Mathf.Abs(oldUIRot) > 180 ? oldUIRot =  oldUIRot - 360 : oldUIRot;
        }
    }

    public void SetImagesPath(string path) 
    {
        puzzle_images_path = new JournalData(path);
    }
  

    public void ClearData() 
    {
        oldUIRot = 0;
        oldUIpos = Vector2.zero;
    }

    public PuzzleData[] GetPage(int p) => puzzle_images_path.GetPage(p);

    public float GetAngle() => oldUIRot;
    public Vector2 GetPos() => oldUIpos;

    public int GetNumberOfPages() => puzzle_images_path.GetNumberOfPages();
}
