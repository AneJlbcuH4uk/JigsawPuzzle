using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPuzzleData : MonoBehaviour
{

    private void Awake()
    {
        preview = transform.GetChild(0).GetComponent<Image>();
        UpdateImage();
    }

    public IEnumerator SetUIPuzzleData(PuzzleData data) 
    {
        puzzle_image = new Texture2D(2, 2);
        puzzle_image.LoadImage(System.IO.File.ReadAllBytes(data.Image));
        mask_type = data.Mt;
        number_of_puzzles_in_height = data.Num;
        offset = data.Off;
        UpdateImage();
        yield return null;
    }

    private void UpdateImage() 
    {
        preview.sprite = Sprite.Create(puzzle_image, new Rect(0, 0, puzzle_image.width, puzzle_image.height), new Vector2(.5f, .5f));
    }

    //public void SetData(UIPuzzleData d) 
    //{
    //    this = d;
    //}

    [SerializeField] private Image preview;
    [SerializeField] private Texture2D puzzle_image;
    [SerializeField] private MaskType mask_type;
    private int max_puzzle_height = 120;

    [Range(2, 30)] [SerializeField] private int number_of_puzzles_in_height;
    [Range(1, 99)] [SerializeField] private int offset;

    public Texture2D GetImage() => puzzle_image;
    public MaskType GetMaskType() => mask_type;
    public int GetNumberofPuzzles() => number_of_puzzles_in_height;
    public int GetOffset() => offset;

    public void SetMaskType(MaskType t) 
    {
        mask_type = t;
    }

    public int[] GetMaxAmountOfPuzzles() 
    {
        int[] res = new int[2];

        res[1] = Mathf.FloorToInt((float)puzzle_image.height / max_puzzle_height);
        res[0] = GetAmountOfPuzzlesInWidth(res[1]);

        return res;
    }

    public int GetAmountOfPuzzlesInWidth(int val) 
    {
        int res = 0;

        if (mask_type == MaskType.Classic)
        {
            res = Mathf.FloorToInt(puzzle_image.width / (puzzle_image.height / val));
        }
        if (mask_type == MaskType.Hex)
        {
            var radius = ((float)puzzle_image.height / val) / Mathf.Sqrt(3);
            res = Mathf.FloorToInt((puzzle_image.width - radius / 2) / (1.5f * radius));
        }
        return res;
    }

    public void SetNumberOfPuzzles(int p)
    {
        number_of_puzzles_in_height = p;
    }

    public void ResetState() 
    {
        gameObject.SetActive(true);
        gameObject.transform.GetChild(2).gameObject.SetActive(false);
    }

}
