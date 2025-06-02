using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PuzzleLoadingDataSerializable
{
    public string puzzle_image_path;
    public MaskType mask_type;
    public int number_of_puzzles_in_height;
    public int offset;
    public Vector2Int max_im_size;
    public int seed;
    public List<Vector3> puzzles_positions;
}


public class PuzzleLoadingData : MonoBehaviour
{
    public string scene_name = "MainGame";
    private string path_to_saves;

    [SerializeField] public bool data_was_set = false;
    [SerializeField] public bool loading_from_file = false;

    [SerializeField] private Texture2D puzzle_image;
    [SerializeField] private MaskType mask_type;
    [SerializeField] private int number_of_puzzles_in_height;
    [SerializeField] private int offset;
    [SerializeField] private Vector2Int max_im_size = new Vector2Int(1440, 810);

    [SerializeField] private int seed;
    [SerializeField] private string path_to_image;

    [SerializeField] private List<Vector3> puzzles_positions;
    [SerializeField] private bool imageloaded = false;

    public string GetSavePath() => path_to_saves;
    public bool IsImageLoaded() => imageloaded;

    void Awake()
    {
        SetPath();
        DontDestroyOnLoad(gameObject);    
    }

    public ref List<Vector3> GetTransforms() 
    {
        return ref puzzles_positions;
    }

    public string GetImageName() 
    {
        return Path.GetFileNameWithoutExtension(path_to_image);
    }

    public string GetImageNameWithExtention()
    {
        return Path.GetFileName(path_to_image);
    }

    public void SetSeed(int st) 
    {
        seed = st;
    }

    public string GetJournalName()
    {
        return Path.GetDirectoryName(path_to_image);
    }

    private void SetPuzzleTransforms() 
    {
        var pg = GameObject.FindGameObjectWithTag("GameManager").GetComponent<PuzzleGeneration>();
        puzzles_positions = pg.GetPuzzlePositions();
    }

    public void SetData(UIPuzzleData d) 
    {
        //puzzle_image = d.GetImage();
        mask_type = d.GetMaskType();
        number_of_puzzles_in_height = d.GetNumberofPuzzles();
        offset = d.GetOffset();
        path_to_image = d.GetImPath();

        puzzle_image = new Texture2D(2, 2);
        puzzle_image.LoadImage(System.IO.File.ReadAllBytes(path_to_image));

        data_was_set = true;
        imageloaded = true;
        loading_from_file = false;
    }

    public void GetSetData(out Texture2D im, out MaskType mt, out int n, out int off) 
    {

        im = puzzle_image;
        mt = mask_type;
        n = number_of_puzzles_in_height;
        off = offset;
       
    }

    public Texture2D GetTexture() => puzzle_image;

    public Vector2 GetImageMeasures() 
    {
        float dx = (float)puzzle_image.width / max_im_size.x;
        float dy = (float)puzzle_image.height / max_im_size.y;

        float d = dx > dy ? dx : dy;

        return new Vector2(puzzle_image.width / d, puzzle_image.height / d);

    }

    public string GetSceneName() => scene_name;

    public void SaveToFile(string save_file_name)
    {
        SetPath();
        SetPuzzleTransforms();


        if (!Directory.Exists(path_to_saves))
        {
            Directory.CreateDirectory(path_to_saves);
        }

        PuzzleLoadingDataSerializable data = new PuzzleLoadingDataSerializable
        {  
            
            puzzle_image_path = path_to_image,
            mask_type = mask_type,
            number_of_puzzles_in_height = number_of_puzzles_in_height,
            offset = offset,
            max_im_size = max_im_size,

            seed = seed,
            puzzles_positions = puzzles_positions
        };

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(Path.Combine(path_to_saves, save_file_name + ".json"), json);
    }

    private void SetPath() 
    {
        path_to_saves = Path.Combine(Application.persistentDataPath, "Saves");
    }
    public int GetSeed() => seed;

    public void LoadFromFile(string save_file_name)
    {
        SetPath();
        imageloaded = false;
        string filePath = Path.Combine(path_to_saves, save_file_name + ".json");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            PuzzleLoadingDataSerializable data = JsonUtility.FromJson<PuzzleLoadingDataSerializable>(json);

            data_was_set = true;
            puzzle_image = LoadTexture(data.puzzle_image_path);
            mask_type = data.mask_type;
            number_of_puzzles_in_height = data.number_of_puzzles_in_height;
            offset = data.offset;
            max_im_size = data.max_im_size;
            seed = data.seed;
            puzzles_positions = data.puzzles_positions;
            loading_from_file = true;
            path_to_image = data.puzzle_image_path;
        }
        else
        {
            Debug.LogWarning("File not found: " + filePath);
        }
    }


    private Texture2D LoadTexture(string filePath)
    {
        if (File.Exists(filePath))
        {
            byte[] bytes = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(bytes);
            imageloaded = true;
            return texture;
        }
        Debug.LogWarning("Texture file not found: " + filePath);
        
        return null;
    }


    

}
