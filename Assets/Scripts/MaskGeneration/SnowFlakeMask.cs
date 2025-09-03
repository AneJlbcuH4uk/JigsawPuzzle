using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowFlakeMask : MaskGenerator
{
    

    private List<Vector2> snowflake = new List<Vector2>();

    public SnowFlakeMask(Vector2Int res, int num, MaskType mt) 
        : base(res, num, mt) { }

    protected override void CreatePuzzleMask(Texture2D c, MaskType t)
    {
        DrawSnowFlakeGrid(c, number_of_puzzles, 6);
    }



    private void DrawSnowflake(Texture2D c, Vector2 center, float radius, float iter, Color color)
    {
        List<Vector2> snowflakepoints = new List<Vector2>();
        snowflakepoints.Add(center + new Vector2(0, radius));
        snowflakepoints.Add(center + new Vector2(radius * Mathf.Cos(-Mathf.PI / 6), radius * Mathf.Sin(-Mathf.PI / 6)));
        snowflakepoints.Add(center + new Vector2(radius * Mathf.Cos(Mathf.PI * 7 / 6), radius * Mathf.Sin(Mathf.PI * 7 / 6)));

        if (snowflake.Count == 0)
        {
            for (int o = 0; o < iter; o++)
            {
                if (Vector2.Distance(snowflakepoints[0], snowflakepoints[1]) < 10) break;

                List<Vector2> temp = new List<Vector2>();
                for (int i = 0; i < snowflakepoints.Count; i++)
                {
                    temp.Add(snowflakepoints[i]);
                    if (i == snowflakepoints.Count - 1)
                    {
                        foreach (var v in AddTriangleonLine(snowflakepoints[snowflakepoints.Count - 1], snowflakepoints[0]))
                            temp.Add(v);
                    }
                    else
                    {
                        foreach (var v in AddTriangleonLine(snowflakepoints[i], snowflakepoints[i + 1]))
                            temp.Add(v);
                    }
                }
                snowflakepoints = temp;
            }

            for (int i_vec = 0; i_vec < snowflake.Count; i_vec++)
            {
                snowflake.Add(snowflakepoints[i_vec] - center);
            }
        }
        else
        {

            for (int i_vec = 0; i_vec < snowflakepoints.Count; i_vec++)
            {
                snowflakepoints.Add(snowflake[i_vec] + center);
            }
        }

        for (int i = 0; i < snowflakepoints.Count; i++)
        {
            if (i == snowflakepoints.Count - 1)
            {
                DrawLine(c, snowflakepoints[snowflakepoints.Count - 1], snowflakepoints[0], color);
            }
            else
            {
                DrawLine(c, snowflakepoints[i], snowflakepoints[i + 1], color);
            }
        }

    }

    private List<Vector2> AddTriangleonLine(Vector2 start, Vector2 end)
    {

        Vector2 point3 = (end - start) * 2 / 3 + start;
        Vector2 point1 = (end - start) / 3 + start;

        float t = Mathf.Atan2(point3.y - point1.y, point3.x - point1.x);

        Vector2 point2 = (point3 - point1) / 2 + point1;

        Vector2 temp = point3 - point2;

        point2 = new Vector2(-temp.y, temp.x) * Mathf.Sqrt(3) + point2;
        List<Vector2> newtriangle = new List<Vector2>() { point1, point2, point3 };

        return newtriangle;
    }


    private void DrawSnowFlakeGrid(Texture2D c, Vector2 numberofp, float iter)
    {
        float radius = c.height / numberofp.y / 2;

        numberofp.x = Mathf.Floor((c.width - 2 * radius) / (radius * 2 * Mathf.Sqrt(3))) + 1;
        float offset_x = ((c.width - radius * 2) - radius * 2 * Mathf.Sqrt(3) * (numberofp.x - 1)) / 2;

        for (int j = -1; j < numberofp.x + 1; j++)
        {
            for (int i = 0; i < numberofp.y + 1; i++)
            {
                Vector2 center;
                if (i < numberofp.y)
                {
                    center = new Vector2(radius * 2 * Mathf.Sqrt(3) * j + radius + offset_x, radius * 2 * i + radius);
                    DrawSnowflake(c, center, radius, iter, Color.black);
                }
                if (j < numberofp.x)
                {
                    center = new Vector2((radius * 2 * Mathf.Sqrt(3) * j + radius) + Mathf.Sqrt(3) * radius + offset_x, radius * 2 * i);
                    DrawSnowflake(c, center, radius, iter, Color.black);
                }
            }
        }

        void CheckLine(int line, Color fill)
        {
            int number_of_white_pixels = 0;
            for (int i = 0; i < c.height; i++)
            {
                if (c.GetPixel(line, i) != Color.black)
                {
                    number_of_white_pixels++;
                }
                else
                {
                    if (number_of_white_pixels < radius / 2)
                    {
                        FillArea(c, line, i - 1, fill);
                    }
                    number_of_white_pixels = 0;
                }
            }
        }

        CheckLine(1, Color.red);
        CheckLine(c.width - 2, Color.red);

        CheckLine((int)(1 + offset_x), Color.white);
        CheckLine((int)(c.width - offset_x - 2), Color.white);

        SwapRedToBlack(c);

        void SwapRedToBlack(Texture2D texture)
        {
            Color[] pixels = texture.GetPixels();

            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i] == Color.red) // Exact match with Color.red
                {
                    pixels[i] = Color.black;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
        }
    }
}
