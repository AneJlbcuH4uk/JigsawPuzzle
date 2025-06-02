using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using System.Threading.Tasks;
using UnityEngine.Networking;
public class UISaveData : MonoBehaviour
{
    private PuzzleLoadingData ManagerDataRef;

    [SerializeField] private Image preview;
    [SerializeField] private Texture2D puzzle_image;
    [SerializeField] private Texture2D loaded_image;
    [SerializeField] private int targetResolution = 256;
    [SerializeField] private GameObject UIBehaviour_obj;
    [SerializeField] private Toggle select_toggle;
    private string path_to_im;
    private TMP_Text time;
    private TMP_Text save_name;
    private TMP_Text puzzle_name;
    private string save_file_path;

    private GameObject holder;

    private Vector2Int res = Vector2Int.zero;

    public IEnumerator SetUISaveData(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning("File not found: " + path);
            yield break;
        }

        // Update UI metadata immediately
        string json = File.ReadAllText(path);
        PuzzleLoadingDataSerializable data = JsonUtility.FromJson<PuzzleLoadingDataSerializable>(json);
        path_to_im = data.puzzle_image_path;
        res = data.max_im_size;

        save_file_path = path;
        save_name.text = Path.GetFileNameWithoutExtension(path);
        puzzle_name.text = Path.GetFileNameWithoutExtension(path_to_im);
        time.text = $"{File.GetLastWriteTime(path):yyyy/MM/dd HH:mm}";

        
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file://" + path_to_im))
        {
            //print("file://" + path_to_im);
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to load texture: {uwr.error}");
                yield break;
            }

            Texture2D tempTexture = DownloadHandlerTexture.GetContent(uwr);

            // Crop the texture to a square
            int squareSize = Mathf.Min(tempTexture.width, tempTexture.height);
            int startX = (tempTexture.width - squareSize) / 2;
            int startY = (tempTexture.height - squareSize) / 2;

            Color[] croppedPixels = tempTexture.GetPixels(startX, startY, squareSize, squareSize);

            Texture2D croppedTexture = new Texture2D(squareSize, squareSize, TextureFormat.RGBA32, false);
            croppedTexture.SetPixels(croppedPixels);
            croppedTexture.Apply();

            // Scale the final texture
            StartCoroutine(ScaleTexture(croppedTexture, targetResolution, targetResolution));         
        }
    }

    public string GetName() 
    {
        return save_name.text;
    }

    private void Awake()
    {
        ManagerDataRef = GameObject.FindGameObjectWithTag("LoadingData").GetComponent<PuzzleLoadingData>();
        preview = transform.GetChild(0).GetComponent<Image>();
        save_name = transform.GetChild(1).GetComponent<TMP_Text>();
        puzzle_name = transform.GetChild(2).GetComponent<TMP_Text>();
        time = transform.GetChild(3).GetComponent<TMP_Text>();
        
        holder = GameObject.FindGameObjectWithTag("SaveFilesHolder");
        UIBehaviour_obj = GameObject.FindGameObjectWithTag("MainCanvas");
        GetComponent<Button>().onClick.AddListener(UIBehaviour_obj.GetComponent<UIBehaviour>().OnSaveLoadClick);
      
    }

    private void UpdateImage()
    {
        preview.sprite = Sprite.Create(puzzle_image, new Rect(0, 0, puzzle_image.width, puzzle_image.height), new Vector2(.5f, .5f));
    }

    private IEnumerator ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        yield return null;
        RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight);
        RenderTexture.active = rt;

        // Draw the source texture to the RenderTexture
        Graphics.Blit(source, rt);

        // Create a new Texture2D from the RenderTexture
        Texture2D result = new Texture2D(targetWidth, targetHeight);
        result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        result.Apply();

        // Release the RenderTexture
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        puzzle_image = result;
        UpdateImage();
    }

    public void OnLoadClick() 
    {  
        ManagerDataRef.LoadFromFile(save_name.text); 
    }

    public void DeleteSave() 
    {
        if (!File.Exists(save_file_path)) 
        {
            return;
        }

        File.Delete(save_file_path);
        Destroy(gameObject);
    }

    
    public void OnSelectToggle()
    {
        var lm = holder.GetComponent<LoadMenuUI>();

        if(select_toggle.isOn)
            lm.AddToSelectedSaves(gameObject.GetComponent<UISaveData>());
        else
            lm.RemoveFromSelectedSaves(gameObject.GetComponent<UISaveData>());
    }

    public void ResetToggle() 
    {
        select_toggle.isOn = false;
    }

    public void SetToggle(bool tog) 
    {
        select_toggle.isOn = tog;
    }

    public void RenameFile(string name) 
    {
        
        string new_path = Path.Combine(Path.GetDirectoryName(save_file_path), name + ".json");

        string uniquePath = new_path;
        int counter = 0;

        while (File.Exists(uniquePath))
        {
            counter++;
            uniquePath = Path.Combine(Path.GetDirectoryName(new_path),
                $"{Path.GetFileNameWithoutExtension(new_path)}_{counter}{Path.GetExtension(new_path)}");            
        }
        save_name.text = counter != 0 ? $"{name}_{counter}" : name;
        File.Move(save_file_path, uniquePath);
        save_file_path = uniquePath;
    }

}
