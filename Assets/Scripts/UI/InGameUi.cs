using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameUi : MonoBehaviour
{
    private PuzzleDataTracker dataTracker;
    [SerializeField] private GameObject UIHolder;
    [SerializeField] private GameObject LoadingScreen;
    [SerializeField] private Material mat ;

    private void Start()
    {
        if (PuzzleLoadingData.data_was_set)
        {
            var image = PuzzleLoadingData.GetTexture();
            var im_obj = LoadingScreen.transform.GetChild(1).GetComponent<Image>();
            im_obj.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);
            im_obj.rectTransform.sizeDelta = PuzzleLoadingData.GetImageMeasures(); 
        }
        LoadingScreen.SetActive(true);
        mat.SetFloat("_CurFillPercent", 0.1f);
        
    }
    public void UpdateLoadState(float loadState)
    {
        mat.SetFloat("_CurFillPercent", loadState);
    }


    public IEnumerator FinishLoad() 
    {
        mat.SetFloat("_CurFillPercent", 1);
        yield return new WaitForSeconds(1);
        LoadingScreen.SetActive(false);
    }

    private void Awake()
    {
        dataTracker = GameObject.FindGameObjectsWithTag("GameManager")[0].GetComponent<PuzzleDataTracker>();
    }

    public void LeaveButton() 
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public void Pause()
    {
        UIHolder.SetActive(!UIHolder.activeSelf);
        dataTracker.SetInteractionBool(UIHolder.activeSelf);
    }

    public bool IsUIActive() => UIHolder.activeSelf;

}
