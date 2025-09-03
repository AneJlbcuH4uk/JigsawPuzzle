using System;
using System.Collections.Generic;
using UnityEngine;

public enum MaskType { Classic, Hex, Tshape, Scale, Sshape, SnowFlake }
public abstract class MaskGenerator
{
    
    protected int side_number = 10;
    protected Vector2Int image_rec = new Vector2Int(1920, 1080);
    protected Texture2D mask;
    protected Vector2 number_of_puzzles;
    protected Vector2 pixel_sides;

    protected abstract void CreatePuzzleMask(Texture2D c, MaskType t);

    public MaskGenerator(Vector2Int res, int num, MaskType mt) 
    {
        side_number = num;
        image_rec = res;
        GenerateMask(mt);
    }

    public Texture2D GetMask() => mask;


    protected void GenerateMask(MaskType mt) 
    {
        mask = GenerateEmpty(image_rec);
        number_of_puzzles = new Vector2(mask.width / (mask.height / side_number), side_number);
        pixel_sides = new Vector2((mask.width / number_of_puzzles.x), (mask.height / number_of_puzzles.y));
        DrawPuzzleMask(mask, mt);   
    }


    protected Texture2D GenerateEmpty(Vector2Int rec) 
    {
        var t = new Texture2D(rec.x, rec.y);
        t.wrapMode = TextureWrapMode.Clamp;
        Color[] colors = new Color[rec.x * rec.y];
        for(int i = 0; i < colors.Length; i++) 
        {
            colors[i] = Color.white;
        }
        t.SetPixels(colors);
        return t;

    }

    protected void SetPixelsInRange(Texture2D c, int x, int y, Color col, bool v)
    {
        if (v) _swap_int(ref x, ref y);

        c.SetPixel(x, y, col);

        if (x - 1 >= 0) c.SetPixel(x - 1, y, col);
        if (x + 1 < c.width) c.SetPixel(x + 1, y, col);
        if (y - 1 >= 0) c.SetPixel(x, y - 1, col);
        if (y + 1 < c.height) c.SetPixel(x, y + 1, col);
    }


    protected void _swap_int(ref int a, ref int b)
    {
        var t = a;
        a = b;
        b = t;
    }


    protected void DrawLine(Texture2D c, Vector2 start, Vector2 end, Color color)
    {
        int x0 = Mathf.RoundToInt(start.x);
        int y0 = Mathf.RoundToInt(start.y);
        int x1 = Mathf.RoundToInt(end.x);
        int y1 = Mathf.RoundToInt(end.y);

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;
        int x = x0, y = y0;

        while (true)
        {
            SetPixelsInRange(c, x, y, color, false);

            if (x == x1 && y == y1)
                break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y += sy;
            }
        }

    }


    protected void FillArea(Texture2D image, int startX, int startY, Color fillcolor)
    {
        if (image == null) return;

        // Get the image dimensions
        int width = image.width;
        int height = image.height;

        // Get the starting pixel color
        Color startColor = image.GetPixel(startX, startY);

        // If the starting pixel is already black, return
        if (startColor == Color.black) return;

        if (startColor == Color.white && fillcolor == Color.white) return;

        // Create a stack to manage the pixels to process
        Stack<Vector2Int> pixels = new Stack<Vector2Int>();
        pixels.Push(new Vector2Int(startX, startY));

        while (pixels.Count > 0)
        {
            Vector2Int current = pixels.Pop();
            int x = current.x;
            int y = current.y;

            // Skip if out of bounds or already black
            if (x < 0 || x >= width || y < 0 || y >= height || image.GetPixel(x, y) == Color.black || image.GetPixel(x, y) == fillcolor)
                continue;

            // Set the current pixel to black
            image.SetPixel(x, y, fillcolor);

            // Add neighbors to the stack
            pixels.Push(new Vector2Int(x + 1, y)); // Right
            pixels.Push(new Vector2Int(x - 1, y)); // Left
            pixels.Push(new Vector2Int(x, y + 1)); // Up
            pixels.Push(new Vector2Int(x, y - 1)); // Down
        }

        // Apply the changes to the texture
        image.Apply();
    }

    

    protected void DrawPuzzleMask(Texture2D c, MaskType t) 
    {
        CreatePuzzleMask(c, t);
        c.Apply();
    }

 

    public static MaskGenerator Create(Vector2Int res, int num, MaskType t) 
    {
        return t switch
        {
            MaskType.Classic => new ClassicMask(res, num, t),
            MaskType.Hex => new HexMask(res, num, t),
            MaskType.Tshape => new TshapeMask(res, num, t),
            MaskType.Sshape => new SshapeMask(res, num, t),
            MaskType.Scale => new ScaleMask(res, num, t),
            MaskType.SnowFlake => new SnowFlakeMask(res, num, t),
            _ => throw new ArgumentException("Invalid type in mask generator")
        };
    }

    
}
