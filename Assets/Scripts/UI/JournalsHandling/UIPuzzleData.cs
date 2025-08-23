using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UIPuzzleData : MonoBehaviour
{
    private Sprite _gray_preview_pic;
    private GameObject completion_sign;

    [SerializeField] private Image preview;
    [SerializeField] private Texture2D puzzle_image;
    [SerializeField] private MaskType mask_type;
    private int max_puzzle_height = 120;
    private string path_to_im;

    [Range(2, 30)] [SerializeField] private int number_of_puzzles_in_height;
    [Range(1, 99)] [SerializeField] private int offset;

    public event Action<int> OnPuzzleCountChanged;
    public event Action<MaskType> OnMaskTypeChanged;

    [SerializeField] private bool _this_puzzle_was_completed = false;
    [SerializeField] private GameObject PuzzleConfigMenu;

    private float puzzle_image_width;
    private float puzzle_image_height;

    public Texture2D GetImage() => puzzle_image;
    public MaskType GetMaskType() => mask_type;
    public int GetNumberofPuzzles() => number_of_puzzles_in_height;
    public int GetOffset() => offset;

    public string GetImPath() => path_to_im;

    private void Awake()
    {
        PuzzleConfigMenu = transform.GetChild(2).gameObject;
        preview = transform.GetChild(0).GetComponent<Image>();
        _gray_preview_pic = Sprite.Create(CreateGrayTexture(380, 250), new Rect(0, 0, 380, 250), new Vector2(.5f, .5f));
        completion_sign = transform.GetChild(3).gameObject;
        //UpdateImage();
    }

    Texture2D CreateGrayTexture(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false); // Create a new texture
        Color grayColor = Color.gray; // Define gray color
        Color[] pixels = new Color[width * height]; // Create an array to hold all pixels

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = grayColor; // Set all pixels to gray
        }

        texture.SetPixels(pixels); // Apply the pixel colors to the texture
        texture.Apply(); // Apply the changes to the texture

        return texture;
    }

    private void ResetConfigToDefault()
    {
        ResetMaskType(MaskType.Classic);
        SetNumberOfPuzzles(4);
        SetCompletionMark(false);
        offset = 5;
    }

    public void SetNumberOfPuzzles(int p)
    {
        if (p != number_of_puzzles_in_height)
        {
            number_of_puzzles_in_height = p;
            OnPuzzleCountChanged?.Invoke(number_of_puzzles_in_height);
        }
    }


    public IEnumerator SetUIPuzzleData(PuzzleData data, bool completed)
    {
        yield return null;
        ResetConfigToDefault();
        if (PuzzleConfigMenu.activeSelf)
        {
            PuzzleConfigMenu.SetActive(false);
            gameObject.GetComponent<Button>().enabled = true;
        }

        preview.sprite = _gray_preview_pic;

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture("file://" + data.Image))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to load texture from {data.Image}: {request.error}");
            }
            else
            {
                mask_type = data.Mt;
                number_of_puzzles_in_height = data.Num;
                offset = data.Off;
                path_to_im = data.Image;
                _this_puzzle_was_completed = completed;
                SetCompletionMark(_this_puzzle_was_completed);

                Texture2D downloadedTexture = DownloadHandlerTexture.GetContent(request);

                puzzle_image_width = downloadedTexture.width;
                puzzle_image_height = downloadedTexture.height;

                //loaded_image_size = downloadedTexture.

                float targetAspect = 380f / 250f;
                float imageAspect = (float)downloadedTexture.width / downloadedTexture.height;

                Rect uvRect;
                if (imageAspect > targetAspect)
                {
                    // too wide -> crop left/right
                    float newWidth = targetAspect / imageAspect; // relative width
                    uvRect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f);
                }
                else
                {
                    // too tall -> crop top/bottom
                    float newHeight = imageAspect / targetAspect; // relative height
                    uvRect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight);
                }

                RenderTexture rt = RenderTexture.GetTemporary(380, 250, 0, RenderTextureFormat.ARGB32);
                RenderTexture.active = rt;

                GL.PushMatrix();
                GL.LoadPixelMatrix(0, 1, 1, 0); // Match UVs
                Graphics.DrawTexture(new Rect(0, 0, 1, 1), downloadedTexture, uvRect, 0, 0, 0, 0);
                GL.PopMatrix();

                Texture2D scaledTexture = new Texture2D(380, 250, TextureFormat.RGBA32, false);
                scaledTexture.ReadPixels(new Rect(0, 0, 380, 250), 0, 0);
                scaledTexture.Apply();

                RenderTexture.ReleaseTemporary(rt);
                RenderTexture.active = null;

                puzzle_image = scaledTexture;
                UpdateImage();

            }
        }
    }

    private void SetCompletionMark(bool state) 
    {
        if(completion_sign == null)
        completion_sign = transform.GetChild(3).gameObject;
        completion_sign.SetActive(state);       
    }

    public void SwitchConfigMenu()
    {
        bool initialState = PuzzleConfigMenu.activeSelf;
        PuzzleConfigMenu.SetActive(!initialState);
        SetCompletionMark(_this_puzzle_was_completed && !PuzzleConfigMenu.activeSelf);
 
    }

    private void UpdateImage()
    {
        preview.sprite = Sprite.Create(puzzle_image, new Rect(0, 0, puzzle_image.width, puzzle_image.height), new Vector2(.5f, .5f));
    }




    public void SetMaskType(MaskType t) 
    {
        mask_type = t;
    }

    public void ResetMaskType(MaskType t)
    {
        if (mask_type != t)
        {
            SetMaskType(t);
            OnMaskTypeChanged?.Invoke(mask_type);
        }
    }


    public int[] GetMaxAmountOfPuzzles() 
    {
        int[] res = new int[2];

        res[1] = Mathf.FloorToInt(puzzle_image_height / max_puzzle_height);
        //print((float)puzzle_image.height);

        res[0] = GetAmountOfPuzzlesInWidth(res[1]);

        return res;
    }

    public int GetAmountOfPuzzlesInWidth(int val) 
    {
        int res = 0;

        if (mask_type == MaskType.Classic)
        {
            res = Mathf.FloorToInt(puzzle_image_width / (puzzle_image_height / val));
        }
        if (mask_type == MaskType.Hex)
        {
            var radius = (puzzle_image_height / val) / Mathf.Sqrt(3);
            res = Mathf.FloorToInt((puzzle_image_width - radius / 2) / (1.5f * radius));
        }
        if (mask_type == MaskType.Tshape)
        {
            res = Mathf.RoundToInt(puzzle_image_width / (puzzle_image_height / val));
        }
        if(mask_type == MaskType.Scale) 
        {
            res = Mathf.FloorToInt(puzzle_image_width / (puzzle_image_height / val));
        }



        return res;
    }

    public int GetTotalNumberOfPuzzles(int val) 
    {
        int res = 0;
        if (mask_type == MaskType.Classic) 
        {
            res = GetAmountOfPuzzlesInWidth(val) * val;
        }
        if (mask_type == MaskType.Tshape)
        {
            res = GetAmountOfPuzzlesInWidth(val) * val * 4;
        }
        if(mask_type == MaskType.Hex) 
        {
            float radius = (puzzle_image_height / val) / Mathf.Sqrt(3);
            float num_in_y = Mathf.Floor((puzzle_image_width - radius / 2) / (1.5f * radius));
            res = (int)(num_in_y * val - (int)num_in_y / 2);
        }
        if (mask_type == MaskType.Scale)
        {
            float radius = puzzle_image_height / val / 2;
            int num_in_y =  Mathf.FloorToInt(puzzle_image_width / radius / 2);
            print(num_in_y);
            res = 2 * val * num_in_y + val;
        }
        if (mask_type == MaskType.SnowFlake)
        {
            float radius = puzzle_image_height / val / 2;
            float num_in_y = Mathf.Floor((puzzle_image_width - 2 * radius) / (radius * 2 * Mathf.Sqrt(3))) + 1;

            res =(int)(val * num_in_y + (val + 1) * (num_in_y + 1)) * 3 - 2 - val * 2;
        }
        if (mask_type == MaskType.Sshape)
        {
            float num_in_y = Mathf.Round(puzzle_image_width / (puzzle_image_height / (float)val));
            res = Mathf.RoundToInt((val * num_in_y * (1f + (13f / 18f)) + (val + num_in_y * (1f + (1f/3f))))*1.2f);
        }

        return res;
    }



    public void ResetState() 
    {
        gameObject.SetActive(true);
        gameObject.transform.GetChild(2).gameObject.SetActive(false);
    }

    public override string ToString()
    {
        return $" UI puzzle data exists path to image = {path_to_im}";
    }


}
