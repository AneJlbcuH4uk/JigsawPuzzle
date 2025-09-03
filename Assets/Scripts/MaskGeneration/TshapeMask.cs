using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TshapeMask : MaskGenerator
{
    public TshapeMask(Vector2Int res, int num, MaskType mt) 
        : base(res, num, mt) { }

    protected override void CreatePuzzleMask(Texture2D c, MaskType t)
    {
        DrawTshapeGrid(c, number_of_puzzles);
    }


    private void DrawTshapeGrid(Texture2D c, Vector2 numberofp)
    {
        numberofp.x = Mathf.Round(c.width / (c.height / numberofp.y));

        for (int i = 1; i < numberofp.x; i++)
        {
            Vector2 start = new Vector2(c.width / numberofp.x * i, 0);
            Vector2 end = new Vector2(c.width / numberofp.x * i, c.height);

            DrawLine(c, start, end, Color.black);
        }

        for (int i = 1; i < numberofp.y; i++)
        {
            Vector2 start = new Vector2(0, c.height / numberofp.y * i);
            Vector2 end = new Vector2(c.width, c.height / numberofp.y * i);

            DrawLine(c, start, end, Color.black);
        }

        for (int i = 0; i < numberofp.y; i++)
        {

            Vector2 offsety = new Vector2(0, c.height / numberofp.y) / 2;

            for (int j = 0; j < numberofp.x; j++)
            {
                float puzzle_width = c.width / numberofp.x;
                float offsetx = puzzle_width / 4;

                Vector2 start = new Vector2(puzzle_width * j + offsetx, c.height / numberofp.y * i) + offsety;
                Vector2 end = new Vector2(puzzle_width * (j + 1) - offsetx, c.height / numberofp.y * i) + offsety;

                DrawLine(c, start, end, Color.black);
            }
        }

        for (int i = 0; i < numberofp.x; i++)
        {

            Vector2 offsetx = new Vector2(c.width / numberofp.x, 0) / 2;

            for (int j = 0; j < numberofp.y; j++)
            {
                float puzzle_height = c.height / numberofp.y;
                float offsety = puzzle_height / 4;

                Vector2 start = new Vector2(c.width / numberofp.x * i, puzzle_height * j + offsety) + offsetx;
                Vector2 end = new Vector2(c.width / numberofp.x * i, puzzle_height * (j + 1) - offsety) + offsetx;

                DrawLine(c, start, end, Color.black);
            }
        }


        for (int i = 0; i < numberofp.y; i++)
        {

            Vector2 offsety = new Vector2(0, c.height / numberofp.y) / 4;

            for (int j = 0; j < numberofp.x; j++)
            {
                float puzzle_width = c.width / numberofp.x;
                float offsetx = puzzle_width / 4;

                Vector2 start = new Vector2(puzzle_width * j + offsetx, c.height / numberofp.y * i) + offsety;
                Vector2 end = new Vector2(puzzle_width * (j + 1) - offsetx * 2, c.height / numberofp.y * i) + offsety;

                DrawLine(c, start, end, Color.black);

                start = new Vector2(puzzle_width * j + offsetx * 3, c.height / numberofp.y * i) + offsety;
                end = new Vector2(puzzle_width * (j + 1), c.height / numberofp.y * i) + offsety;

                DrawLine(c, start, end, Color.black);

                start = new Vector2(puzzle_width * j, c.height / numberofp.y * i) + offsety * 3;
                end = new Vector2(puzzle_width * (j + 1) - offsetx * 3, c.height / numberofp.y * i) + offsety * 3;

                DrawLine(c, start, end, Color.black);

                start = new Vector2(puzzle_width * j + offsetx * 2, c.height / numberofp.y * i) + offsety * 3;
                end = new Vector2(puzzle_width * (j + 1) - offsetx, c.height / numberofp.y * i) + offsety * 3;

                DrawLine(c, start, end, Color.black);

            }
        }

        for (int i = 0; i < numberofp.x; i++)
        {

            Vector2 offsetx = new Vector2(c.width / numberofp.x, 0) / 4;

            for (int j = 0; j < numberofp.y; j++)
            {
                float puzzle_height = c.height / numberofp.y;
                float offsety = puzzle_height / 4;

                Vector2 start = new Vector2(c.width / numberofp.x * i, puzzle_height * j + offsety * 2) + offsetx;
                Vector2 end = new Vector2(c.width / numberofp.x * i, puzzle_height * (j + 1) - offsety) + offsetx;

                DrawLine(c, start, end, Color.black);

                start = new Vector2(c.width / numberofp.x * i, puzzle_height * j) + offsetx;
                end = new Vector2(c.width / numberofp.x * i, puzzle_height * (j + 1) - offsety * 3) + offsetx;

                DrawLine(c, start, end, Color.black);



                start = new Vector2(c.width / numberofp.x * i, puzzle_height * j + offsety * 3) + offsetx * 3;
                end = new Vector2(c.width / numberofp.x * i, puzzle_height * (j + 1)) + offsetx * 3;

                DrawLine(c, start, end, Color.black);

                start = new Vector2(c.width / numberofp.x * i, puzzle_height * j + offsety) + offsetx * 3;
                end = new Vector2(c.width / numberofp.x * i, puzzle_height * (j + 1) - offsety * 2) + offsetx * 3;

                DrawLine(c, start, end, Color.black);
            }
        }
    }
}
