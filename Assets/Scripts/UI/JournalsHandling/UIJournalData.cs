
using UnityEngine;

public class UIJournalData : MonoBehaviour
{
    
    [SerializeField] private Vector2 oldUIpos;
    [SerializeField] private float oldUIRot;
    [SerializeField] private JournalData puzzle_images_data;

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

    public string GetPath() => puzzle_images_data.GetPath();

    public void SetImagesPath(string path) 
    {
        puzzle_images_data = new JournalData(path);
    }
  


    public void ClearData() 
    {
        oldUIRot = 0;
        oldUIpos = Vector2.zero;
    }

    public PuzzleData[] GetPage(int p) => puzzle_images_data.GetPage(p);

    public float GetAngle() => oldUIRot;
    public Vector2 GetPos() => oldUIpos;

    public Vector2Int GetJournalCompletionNumber() => puzzle_images_data.GetJournalCompletionNumber();

    public bool is_puzzle_completed(string name) => puzzle_images_data.is_puzzle_completed(name);

    public int GetNumberOfPages() => puzzle_images_data.GetNumberOfPages();
}
