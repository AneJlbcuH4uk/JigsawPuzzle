using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


enum Side: int
{
    Left = -1,
    Right = 1
}


public class PuzzleGeneration : MonoBehaviour
{
    private Color GetColor() => new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));

    [SerializeField] private Texture2D image;
    [SerializeField] private GameObject puzzle_prefab;
    [SerializeField] private List<GameObject> puzzles;
    [SerializeField] private int offset;
    [SerializeField] private int number_of_puzzles_in_height;
    [SerializeField] private MaskType mask_type;
    [SerializeField] private GameObject puzzle_holder;
    [SerializeField] private Color outline_color = Color.white;
    [SerializeField] private int outline_width = 4;

    private PuzzleLoadingData pld;

    private Vector2 image_offset = new Vector2 (6,3);
    private Texture2D mask;

    private int[,] segments_marking;
    private List<RectInt> puzzle_shapes;
    private PuzzleDataTracker data_tracker;

    private float loading_time;
    int offset_for_thickness = 15;
    [SerializeField] private float load_state = 0;
    InGameUi inGameUi;

    private bool is_loading = true;
    private bool loading_from_save_file = false;

    public void SetPLD(PuzzleLoadingData set) 
    {
        pld = set;
    }
    public PuzzleLoadingData GetPLD() 
    {
        return pld;
    }

    

    public bool Is_Loading() 
    {
        return is_loading;
    }

    private float startloadtime;
    private List<Vector3> pp = new List<Vector3>();
    public List<Vector3> GetPuzzlePositions() 
    {
        pp.Clear();
        foreach (var ob in puzzles) 
        {
            pp.Add(ob.transform.position);
        }
        return pp;
    }


    private IEnumerator Start()
    {
        pld = GameObject.FindGameObjectWithTag("LoadingData").GetComponent<PuzzleLoadingData>();

        startloadtime = Time.realtimeSinceStartup;
        load_state = 0.1f;
        inGameUi = gameObject.GetComponent<InGameUi>();
        data_tracker = gameObject.GetComponent<PuzzleDataTracker>();

        if (pld.data_was_set)
        {
            pld.GetSetData(out image, out mask_type, out number_of_puzzles_in_height, out offset);
            loading_from_save_file = pld.loading_from_file;
        }

        if (loading_from_save_file) 
        {
            UnityEngine.Random.InitState(pld.GetSeed());  
        }
        else 
        {
            int seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            pld.SetSeed(seed);
            UnityEngine.Random.InitState(seed);
        }

        var mg = new MaskGenerator(new Vector2Int(image.width, image.height), number_of_puzzles_in_height, mask_type);

        mask = mg.GetMask();
        mask.filterMode = FilterMode.Point;

        for (int i = 0; i < mask.width; i++)
        {
            mask.SetPixel(i, 0, Color.clear);
            mask.SetPixel(i, mask.height - 1, Color.clear);
        }
        for (int i = 0; i < mask.height; i++)
        {
            mask.SetPixel(0, i, Color.clear);
            mask.SetPixel(mask.width - 1, i, Color.clear);
        }
        mask.Apply();

        segments_marking = new int[mask.width, mask.height];
        puzzle_shapes = new List<RectInt>();
  
        int segment_number = 1;
        int totalSteps = 2;
        float stepIncrement = 0.85f / totalSteps;

        // Generate segments and update load_state
        yield return StartCoroutine(GenerateSegments(segment_number, stepIncrement));

        // Check lines and update load_state
        yield return StartCoroutine(CheckLines(stepIncrement));

        data_tracker.SetNumberOfPuzzles(puzzle_shapes.Count);

        if (loading_from_save_file)
        {
            List<Vector3> pp = pld.GetTransforms();
            for(int i = 0; i < puzzles.Count; i++) 
            {
                puzzles[i].GetComponent<PuzzlePiece>().ConnectOnLoad(pp[i]);
            }
        }
        else
        {
            ShufflePuzzles();  
        }

        load_state = 1f;
        inGameUi.UpdateLoadState(load_state);
        MouseControl.GetInstance().SetPuzzles(puzzles);
        StartCoroutine(inGameUi.FinishLoad());
        
        yield return null;
        is_loading = false;
        print("LoadingTime: " + (Time.realtimeSinceStartup - startloadtime));
    }

    private IEnumerator GenerateSegments(int segment_number, float stepIncrement)
    {
        
        yield return StartCoroutine(FloodFillWhiteAreas(mask, stepIncrement));

        foreach (var ob in puzzle_shapes)
        {
            CreatePuzzle(ob, segment_number);
            segment_number += 1;
            yield return null;
        }

        yield return null;
    }

    private IEnumerator CheckLines(float stepIncrement)
    {
        int totalLines = (image.height / offset) + (image.width / offset);
        int blockSize = totalLines / 10;  // Adjust this value to control how frequently load_state is updated
        int processedLines = 0;

        for (int j = 1; j < image.height; j += offset)
        {
            StartCoroutine(CheckLine(j, image.width, true));
            processedLines++;

            // Update load_state after processing a block of lines
            if (processedLines % blockSize == 0)
            {
                load_state += stepIncrement * ((float)processedLines / totalLines);
                inGameUi.UpdateLoadState(load_state);

                yield return null;
            }
        }

        for (int j = 1; j < image.width; j += offset)
        {
            StartCoroutine(CheckLine(j, image.height, false));
            processedLines++;

            if (processedLines % blockSize == 0)
            {
                load_state += stepIncrement * ((float)processedLines / totalLines);
                inGameUi.UpdateLoadState(load_state);

                yield return null;
            }
        }
    }


    public Texture2D GetImage() => image;

    public Vector3 GetCenter() 
    {
        Vector3 center = Vector3.zero;

        if (mask_type == MaskType.Hex)
        {
            foreach (var obj in puzzles)
            {
                center += obj.transform.position;
            }
            center /= puzzles.Count;
        }
        if (mask_type == MaskType.Classic || mask_type == MaskType.Tshape || mask_type == MaskType.Sshape || mask_type == MaskType.SnowFlake || mask_type == MaskType.Scale) 
        {
            center = puzzles[0].transform.position - new Vector3((float)(puzzle_shapes[0].width - 1) / 200, (float)(puzzle_shapes[0].height - 1 - offset_for_thickness) / 200, 0);
            center += new Vector3((float)(image.width - 2) / 200, (float)(image.height - 2) / 200);
        }   

         return center;
    }
    
    public void DisablePuzzles() 
    {
        foreach(var obj in puzzles) 
        {
            obj.SetActive(false);
        }
    }

    private List<int[]> possible_pairs = new List<int[]>();

    private void AddPair(int puzzle_index_1, int puzzle_index_2)
    {
        if(possible_pairs.Any(elem => (elem[0] == puzzle_index_1 && elem[1] == puzzle_index_2) 
                                    ||(elem[0] == puzzle_index_2 && elem[1] == puzzle_index_1)))
        {
            var data_1 = puzzles[puzzle_index_1 - 1].GetComponent<PuzzlePiece>();
            var data_2 = puzzles[puzzle_index_2 - 1].GetComponent<PuzzlePiece>();

            data_1.Add_neighbour(puzzle_index_2, data_2.GetCenter(), data_2.GetCollider());
            data_2.Add_neighbour(puzzle_index_1, data_1.GetCenter(), data_1.GetCollider());

            possible_pairs.RemoveAll(elem => (elem[0] == puzzle_index_1 && elem[1] == puzzle_index_2)
                                          || (elem[0] == puzzle_index_2 && elem[1] == puzzle_index_1));
        }
        else 
        {
            possible_pairs.Add(new int[] { puzzle_index_1, puzzle_index_2 });          
        }

        
    }

    private void ShufflePuzzles()
    {
        float loading_change = 0.1f / puzzles.Count;

        float w = image.width / 140;
        float h = image.height / 140;

        foreach (var t in puzzles)
        {
            Vector2 random_pos = new Vector2(UnityEngine.Random.Range(-w, w), UnityEngine.Random.Range(-h, h));
            var pp = t.GetComponent<PuzzlePiece>();

            MouseControl.GetInstance().SetHoldedPuzzle(pp);
            pp.MovePuzzle(random_pos - pp.GetCenter());
            MouseControl.GetInstance().UnsetHoldedPuzzle();
        }
    }

    

    IEnumerator CheckLine(int index, int length, bool isRow)
    {
        int last_index = 0;
        bool was_zero = false;
        int last_zero = 0;

        for (int i = 0; i < length; i++)
        {
            int value = isRow ? segments_marking[i, index] : segments_marking[index, i];
            if (value != 0)
            {
                if (was_zero)
                {

                    if (last_index != 0 && last_index != value && i - last_zero < 7)
                    {
                        AddPair(last_index, value);
                    }
                    last_index = value;
                    was_zero = false;
                }
            }
            else
            {
                if (!was_zero)
                    last_zero = i;
                was_zero = true;
            }
        }
        yield return null;
    }

    void CreatePuzzle(RectInt s, int segment_index)
    {   
        var temp = new Texture2D(s.xMax - s.xMin + 2, s.yMax - s.yMin + 2 + offset_for_thickness, TextureFormat.RGBA32, false);
        
        temp.wrapMode = TextureWrapMode.Clamp;
        
        temp.SetPixels(1, 1 + offset_for_thickness, s.xMax - s.xMin, s.yMax - s.yMin, image.GetPixels(s.xMin, s.yMin, s.xMax - s.xMin, s.yMax - s.yMin));

        for (int i = 0; i < temp.width; i++)
        {
            for (int j = 0; j <= offset_for_thickness;j++)
                temp.SetPixel(i, 0 + j, Color.clear);
            temp.SetPixel(i, temp.height - 1, Color.clear);
        }
        for (int i = 0; i < temp.height; i++)
        {
            temp.SetPixel(0, i, Color.clear);
            temp.SetPixel(temp.width - 1, i, Color.clear);
        }

        for (int i = s.xMin; i < s.xMax; i++)
        {
            for (int j = s.yMin; j < s.yMax; j++)
            {
                if (segments_marking[i, j] != segment_index)
                {
                    temp.SetPixel(i - s.xMin + 1, j - s.yMin + 1 + offset_for_thickness, Color.clear);
                }
            }
        }
        temp.Apply();

        Vector2 place = new Vector2(s.xMax + s.xMin, s.yMax + s.yMin) / 200 - image_offset;

        

        var obj = Instantiate(puzzle_prefab, place, Quaternion.identity, puzzle_holder.transform);
        puzzles.Add(obj);


        var pp = obj.GetComponent<PuzzlePiece>();
        var sr = obj.GetComponent<SpriteRenderer>();

        pp.SetIndex(segment_index);
        pp.SetCenter(place);
        sr.sprite = Sprite.Create(temp, new Rect(0, 0, temp.width, temp.height), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);
        GenerateOutline(obj, temp);

        //obj.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = sr.sprite;

        StartCoroutine(UpdateShapeToSprite(obj));

        //yield return null;
    }
    private void GenerateOutline(GameObject puzzle, Texture2D img)
    {
        // Create the new texture
        int tempWidth = img.width + outline_width * 2;
        int tempHeight = img.height + outline_width * 2;
        Texture2D temp = new Texture2D(tempWidth, tempHeight, TextureFormat.RGBA32, false);

        // Initialize the clear background
        Color[] clearColors = new Color[tempWidth * tempHeight];
        for (int i = 0; i < clearColors.Length; i++)
        {
            clearColors[i] = Color.clear;
        }

        // Fill the brush with the outline color
        Color[] brush = new Color[outline_width * outline_width * 4];
        for (int i = 0; i < brush.Length; i++)
        {
            brush[i] = outline_color;
        }

        // Copy original image pixels into an array for faster access
        Color[] imgPixels = img.GetPixels();
        Color[] tempPixels = clearColors; // Start with a cleared array

        // Check neighboring pixels and draw the outline
        for (int y = 1; y < img.height - 1; y++)
        {
            for (int x = 1; x < img.width - 1; x++)
            {
                int imgIndex = y * img.width + x;
                if (imgPixels[imgIndex] != Color.clear && IsEmptyNearby(imgPixels, x, y, img.width, img.height))
                {
                    DrawSquare(tempPixels, x + outline_width, y + outline_width, tempWidth, brush, outline_width);
                }
            }
        }

        // Apply the pixels to the new texture
        temp.SetPixels(tempPixels);
        temp.Apply();

        // Set the texture as the sprite for the puzzle
        puzzle.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite =
            Sprite.Create(temp, new Rect(0, 0, temp.width, temp.height), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);

        bool IsEmptyNearby(Color[] pixels, int x, int y, int width, int height)
        {
            return pixels[(y - 1) * width + x] == Color.clear ||
                   pixels[(y + 1) * width + x] == Color.clear ||
                   pixels[y * width + (x - 1)] == Color.clear ||
                   pixels[y * width + (x + 1)] == Color.clear;
        }

        void DrawSquare(Color[] pixels, int centerX, int centerY, int texWidth, Color[] brush, int brushSize)
        {
            int startX = centerX - outline_width;
            int startY = centerY - outline_width;
            int brushWidth = brushSize * 2;

            for (int y = 0; y < brushWidth; y++)
            {
                for (int x = 0; x < brushWidth; x++)
                {
                    int pixelIndex = (startY + y) * texWidth + (startX + x);
                    pixels[pixelIndex] = brush[y * brushWidth + x];
                }
            }
        }
    }

    private IEnumerator UpdateShapeToSprite(GameObject obj)
    {
        Destroy(obj.GetComponent<BoxCollider2D>(), 1);
        var t = obj.AddComponent<PolygonCollider2D>();
        t.isTrigger = true;
        obj.GetComponent<PuzzlePiece>().Set_collider(t);
   
        yield return null;
    }

    private bool CloseToWhite(Color c)
    {
        return c.b + c.g + c.r > 2.8;
    }

    private IEnumerator FloodFillWhiteAreas(Texture2D texture,float stepIncrement)
    {
        puzzle_shapes = new List<RectInt>();
        int width = texture.width;
        int height = texture.height;

        // Create a buffer to store the filled pixels
        Color[] pixels = texture.GetPixels();

        // Create a stack for flood fill
        Stack<Vector2Int> stack = new Stack<Vector2Int>();

        // Dictionary to store the color for each area
        Dictionary<int, Color> areaColors = new Dictionary<int, Color>();

        int areaId = 1;
        int processedPixels = 0;
        int totalPixels = width * height;
        int blockSize = totalPixels / 100;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (processedPixels % blockSize == 0)
                {
                    load_state = Mathf.Min(0.1f + stepIncrement * ((float)processedPixels / totalPixels), 1f);
                    inGameUi.UpdateLoadState(load_state);

                    yield return null;
                }
                processedPixels++;


                if (CloseToWhite(pixels[y * width + x]) && segments_marking[x,y] == 0)
                {
                    // Generate a random color for the new area
                    Color randomColor = UnityEngine.Random.ColorHSV();

                    // Start flood fill from this white pixel
                    stack.Push(new Vector2Int(x, y));
                    areaColors[areaId] = randomColor;

                    RectInt res = new RectInt();
                    res.xMin = x; res.xMax = x;
                    res.yMin = y; res.yMax = y;

                    while (stack.Count > 0)
                    {
                        Vector2Int pos = stack.Pop();
                        int px = pos.x;
                        int py = pos.y;

                        if (px < 0 || px >= width || py < 0 || py >= height)
                            continue;

                        int index = py * width + px;
                        if (segments_marking[px, py] != 0 || !CloseToWhite(pixels[index]))
                            continue;

                        // Mark as visited and fill the color
                        segments_marking[px, py] = areaId;
                        pixels[index] = randomColor;

                        RecalculateBorderx(px);
                        RecalculateBordery(py);

                        // Add neighboring pixels to stack
                        stack.Push(new Vector2Int(px + 1, py));
                        stack.Push(new Vector2Int(px - 1, py));
                        stack.Push(new Vector2Int(px, py + 1));
                        stack.Push(new Vector2Int(px, py - 1));
                    }

                    puzzle_shapes.Add(res);
                 
                    areaId++;

                    void RecalculateBorderx(int x)
                    {
                        res.xMin = res.xMin > x ? x : res.xMin;
                        res.xMax = res.xMax < x ? x : res.xMax;
                    }

                    void RecalculateBordery(int y)
                    {
                        res.yMin = res.yMin > y ? y : res.yMin;
                        res.yMax = res.yMax < y ? y : res.yMax;
                    }


                }
            }
        }

        // Set the filled pixels back to the texture
        texture.SetPixels(pixels);
        texture.Apply();

        yield return null;
    }


    float old_offset = 0;
    public void DisableShader() 
    {
        var mat = puzzle_prefab.GetComponent<SpriteRenderer>().sharedMaterial;
        old_offset = mat.GetFloat("_MoveAmount");
        mat.SetFloat("_MoveAmount", 0);
    }

    public void EnableShader() 
    {
        var mat = puzzle_prefab.GetComponent<SpriteRenderer>().sharedMaterial;
        mat.SetFloat("_MoveAmount", old_offset);
    }
    
}
