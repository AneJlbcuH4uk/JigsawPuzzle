using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMask : MaskGenerator
{
    public HexMask(Vector2Int res, int num, MaskType mt)
    : base(res, num, mt) { }

    protected override void CreatePuzzleMask(Texture2D c, MaskType t)
    {
        DrawHexGrid(c, pixel_sides.y, number_of_puzzles);
    }


    private void DrawHexGrid(Texture2D c, float side_x, Vector2 numberofp)
    {
        float radius = side_x / Mathf.Sqrt(3);
        numberofp.x = (c.width - radius / 2) / (3f * radius);
        float shift = (c.width - radius / 2 - (numberofp.x % 1 >= 0.5 ? (int)numberofp.x + 0.5f : (int)numberofp.x) * radius * 3f) / 2;

        for (float i = -side_x / 2; i <= c.height + side_x / 2; i += side_x)
        {
            for (float j = shift - radius * 3; j <= c.width; j += radius * 3)
            {
                if (j < c.width - radius * 1.5f)
                {
                    DrawHex(c, new Vector2(j + radius, i), radius, Color.black);
                }
                else
                {
                    DrawHex(c, new Vector2(j + radius, i), radius, Color.black, true);
                }
                if (j >= shift - 1 && i >= side_x / 2 && i <= c.height - side_x && j <= c.width - radius * 3 + 1)
                {
                    DrawHex(c, new Vector2(j + 5 * radius / 2, i + side_x / 2), radius, Color.black);
                }
                else
                {
                    DrawHex(c, new Vector2(j + 5 * radius / 2, i + side_x / 2), radius, Color.black, true);
                }
            }
        }
    }


    private void DrawHex(Texture2D c, Vector2 center, float radius, Color col, bool fill = false)
    {
        Vector2 st = center + new Vector2(radius * Mathf.Cos(0), radius * Mathf.Sin(0));

        for (float r = radius; r >= 0; r -= 1.5f)
        {
            for (float t = Mathf.PI / 3; t < 2 * Mathf.PI; t += Mathf.PI / 3)
            {
                var temp = center + new Vector2(r * Mathf.Cos(t), r * Mathf.Sin(t));
                DrawLine(c, st, temp, Color.black);
                st = temp;
            }
            if (!fill)
            {
                break;
            }
        }
    }
}
