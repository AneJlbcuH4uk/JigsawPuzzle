using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SshapeMask : MaskGenerator
{
    public SshapeMask(Vector2Int res, int num, MaskType mt)
         : base(res, num, mt) { }

    protected override void CreatePuzzleMask(Texture2D c, MaskType t)
    {
        DrawSshapeGrid(c, number_of_puzzles, 0.05f);
    }


    private void DrawSshapeGrid(Texture2D c, Vector2 numberofp, float line_spawn_probability)
    {
        numberofp.x = Mathf.Round(c.width / (c.height / numberofp.y));
        float line_len_y = (c.height / numberofp.y) * 2 / 6;
        float line_len_x = (c.width / numberofp.x) * 2 / 6;

        Vector2 start = new Vector2(0, line_len_y * 2);
        Vector2 end;

        List<Vector2> startpoints = new List<Vector2>();

        for (int i = 3; start.y <= c.height + c.width; i--)
        {
            end = new Vector2(line_len_x * i, 0) + start;
            DrawLine(c, start, end, Color.black);
            startpoints.Add(end);

            start += new Vector2(0, line_len_y * 3);
            if (i == 1)
            {
                i = 5;
                start -= new Vector2(0, line_len_y * 2);
            }
        }

        foreach (var v in startpoints)
        {
            end = v;
            float t;
            while (end.y > 0 || end.x < c.width)
            {
                start = end;
                end = start + new Vector2(0, -line_len_y * 2);
                DrawLine(c, start, end, Color.black);

                t = Random.Range(0f, 1f);
                if (t < line_spawn_probability) DrawLine(c, start, start + new Vector2(0, line_len_y), Color.black);


                start = end;
                end = start + new Vector2(line_len_x * 4, 0);
                DrawLine(c, start, end, Color.black);

                t = Random.Range(0f, 1f);
                if (t < line_spawn_probability) DrawLine(c, start, start - new Vector2(0, line_len_y), Color.black);
            }
        }

        startpoints.Clear();
        start = new Vector2(line_len_x, 0);

        for (int i = 4; start.x <= c.height + c.width; i++)
        {
            end = new Vector2(0, line_len_y * i) + start;
            DrawLine(c, start, end, Color.black);
            startpoints.Add(end);

            start += new Vector2(line_len_x * 3, 0);
            if (i == 4)
            {
                i = 0;
                start -= new Vector2(line_len_x * 2, 0);
            }
        }

        start = new Vector2(-2 * line_len_x, 0);

        for (int i = 3; start.x > -c.height; i--)
        {
            end = new Vector2(0, line_len_y * i) + start;
            DrawLine(c, start, end, Color.black);
            startpoints.Add(end);

            start -= new Vector2(line_len_x * 3, 0);
            if (i == 0)
            {
                i = 4;
                start += new Vector2(line_len_x * 2, 0);
            }
        }
        foreach (var v in startpoints)
        {
            end = v;
            float t;
            while (end.y < c.height || end.x < c.width)
            {
                start = end;
                end = start + new Vector2(line_len_x * 2, 0);
                DrawLine(c, start, end, Color.black);

                t = Random.Range(0f, 1f);
                if (t < line_spawn_probability) DrawLine(c, start, start - new Vector2(line_len_x, 0), Color.black);


                start = end;
                end = start + new Vector2(0, line_len_y * 4);
                DrawLine(c, start, end, Color.black);

                t = Random.Range(0f, 1f);
                if (t < line_spawn_probability) DrawLine(c, start, start + new Vector2(line_len_x, 0), Color.black);

            }
        }
    }
}
