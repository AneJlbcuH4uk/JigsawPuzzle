using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleMask : MaskGenerator
{
    public ScaleMask(Vector2Int res, int num, MaskType mt)
     : base(res, num, mt) { }

    protected override void CreatePuzzleMask(Texture2D c, MaskType t)
    {
        DrawScaleGrid(c, number_of_puzzles);
    }

    private void DrawScaleGrid(Texture2D c, Vector2 numberofp)
    {
        float radius = c.height / numberofp.y / 2;
        numberofp.x = Mathf.FloorToInt(c.width / radius / 2);
        float offset = (c.width - numberofp.x * radius * 2) / 2;
        for (int i = 0; i < numberofp.y; i++)
        {
            for (int j = 0; j < numberofp.x; j++)
            {
                Vector2 center = new Vector2(j * 2 * radius + radius + offset, i * 2 * radius + radius);
                Vector2 lim = new Vector2(-Mathf.PI / 2, Mathf.PI / 2);
                DrawCircle(c, center, radius, Color.black, lim);
            }
        }

        for (int i = 0; i <= numberofp.x; i++)
        {
            for (int j = 0; j < numberofp.y; j++)
            {
                Vector2 center = new Vector2(i * 2 * radius + offset, j * radius * 2);
                Vector2 lim = new Vector2(-Mathf.PI / 2, Mathf.PI / 2);
                DrawCircle(c, center, radius, Color.black, lim);
            }
        }

        c.Apply();

        for (int i = 0; i <= numberofp.x; i++)
        {
            Vector2 center = new Vector2(i * 2 * radius + offset, c.height - 1);
            FillArea(c, (int)center.x, (int)center.y, Color.black);
        }


    }

    private void DrawCircle(Texture2D c, Vector2 center, float radius, Color color, Vector2 circle_limits)
    {
        for (float angle = circle_limits.x; angle < circle_limits.y; angle += Mathf.PI / 720)
        {
            int x = Mathf.RoundToInt(Mathf.Sin(angle) * radius + center.x);
            int y = Mathf.RoundToInt(Mathf.Cos(angle) * radius + center.y);
            SetPixelsInRange(c, x, y, color, false);
        }
    }

}
