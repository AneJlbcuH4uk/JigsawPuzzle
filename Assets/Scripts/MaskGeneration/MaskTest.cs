using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaskTest : MonoBehaviour
{
    [SerializeField] Vector2Int resolution = new Vector2Int(1920, 1080);
    [SerializeField] int maxJumpDistance = 2;
    [SerializeField] float fillStepDelay = 0.001f;  // Delay between each flood fill step
    [SerializeField] MaskType maskType;


    private Texture2D outputTexture;

    void Start()
    {
        var mg = MaskGenerator.Create(resolution, maxJumpDistance, maskType);
        var maskTexture = mg.GetMask();
        var img = gameObject.GetComponent<Image>();

        // Create a new texture with the same dimensions as the mask texture
        outputTexture = new Texture2D(maskTexture.width, maskTexture.height, TextureFormat.RGBA32, false);

        // Initialize the output texture with the mask texture's pixels
        Color[] pixels = maskTexture.GetPixels();
        outputTexture.SetPixels(pixels);
        outputTexture.Apply();

        // Start the flood fill process
        StartCoroutine(FloodFillWhiteAreas(outputTexture));

        img.sprite = Sprite.Create(outputTexture, new Rect(0, 0, resolution.x, resolution.y), new Vector2(.5f, .5f));
        img.SetNativeSize();
    }

    private IEnumerator FloodFillWhiteAreas(Texture2D texture)
    {
        var puzzle_shapes = new List<RectInt>();
        int width = texture.width;
        int height = texture.height;

        // Create a buffer to store the filled pixels
        Color[] pixels = texture.GetPixels();
        int[] visited = new int[width * height];

        // Create a stack for flood fill
        Stack<Vector2Int> stack = new Stack<Vector2Int>();

        // Dictionary to store the color for each area
        Dictionary<int, Color> areaColors = new Dictionary<int, Color>();

        int areaId = 1;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (IsWhite(pixels[y * width + x]) && visited[y * width + x] == 0)
                {
                    // Generate a random color for the new area
                    Color randomColor = Random.ColorHSV();

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
                        if (visited[index] != 0 || !IsWhite(pixels[index]))
                            continue;

                        // Mark as visited and fill the color
                        visited[index] = areaId;
                        pixels[index] = randomColor;

                        RecalculateBorderx(px);
                        RecalculateBordery(py);

                        // Apply texture changes and add delay for visualization
                        texture.SetPixels(pixels);
                        texture.Apply();

                        yield return new WaitForSeconds(fillStepDelay);  // Delay between steps

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

        print(Mathf.Max(visited));
    }

    private bool IsWhite(Color c)
    {
        // Consider a pixel white if it is close to white
        return Mathf.Approximately(c.r, 1f) && Mathf.Approximately(c.g, 1f) && Mathf.Approximately(c.b, 1f);
    }
}


