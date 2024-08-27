using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class PuzzleLoadingData
{
    public static string scene_name = "MainGame";
    public static bool data_was_set = false;
    
    private static Texture2D puzzle_image;
    private static MaskType mask_type;
    private static int number_of_puzzles_in_height;
    private static int offset;
    private static Vector2Int max_im_size = new Vector2Int(1440, 810);


    public static void SetData(UIPuzzleData d) 
    {
        puzzle_image = d.GetImage();
        mask_type = d.GetMaskType();
        number_of_puzzles_in_height = d.GetNumberofPuzzles();
        offset = d.GetOffset();
        data_was_set = true;
    }

    public static void SetData(out Texture2D im, out MaskType mt, out int n, out int off) 
    {

        im = puzzle_image;
        mt = mask_type;
        n = number_of_puzzles_in_height;
        off = offset;

    }

    public static Texture2D GetTexture() => puzzle_image;

    public static Vector2 GetImageMeasures() 
    {
        float dx = (float)puzzle_image.width / max_im_size.x;
        float dy = (float)puzzle_image.height / max_im_size.y;

        //Debug.Log("dx = " + dx + " |dy = " + dy + " max_im_size" + max_im_size + " image size = " + puzzle_image.width + " " + puzzle_image.height);

        float d = dx > dy ? dx : dy;

        //return max_im_size;
        return new Vector2(puzzle_image.width / d, puzzle_image.height / d);

    }


}
