using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

enum Side: int
{
    Left = -1,
    Right = 1
}


public class Util : MonoBehaviour
{
    [SerializeField] private Texture2D mask;
    [SerializeField] private Texture2D image;
    [SerializeField] private GameObject puzzle_prefab;

    //private Color[] image_pixels;
    private Texture2D mask_copy;
    private int[,] segments_marking;
    private List<RectInt> puzzle_shapes;

    [SerializeField] private List<GameObject> puzzles;
    
    private int segement_number = 1;
    private void Start()
    {
        mask_copy = ChangeFormat(mask, TextureFormat.ARGB32);
        mask_copy.filterMode = FilterMode.Point;
      
        // initializing array of puzzle indexes, and shapes of puzzles
        segments_marking = new int[mask_copy.width, mask_copy.height];
        puzzle_shapes = new List<RectInt>();

        for (int i = 0; i < mask.width; i++)
        {
            for (int j = 0; j < mask.height; j++)
            {
                if (mask_copy.GetPixel(i, j) == Color.white)
                {
                    var temp = SelectSegment(mask_copy, i, j, Color.black, segement_number);
                    puzzle_shapes.Add(temp);
                    StartCoroutine(CreatePuzzle(temp, segement_number));
                    segement_number += 1;                
                }
            }
        }
   
        print("generation time = " + Time.realtimeSinceStartup);
        gameObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create(mask_copy,
                                                                        new Rect(0, 0, mask_copy.width, mask_copy.height),
                                                                        new Vector2(.5f, .5f));
    }


    public Texture2D ChangeFormat(Texture2D oldTexture, TextureFormat newFormat)
    {
        //Create new empty Texture
        Texture2D newTex = new Texture2D(oldTexture.width, oldTexture.height, newFormat, false);
        //Copy old texture pixels into new one
        newTex.SetPixels(oldTexture.GetPixels());
        //Apply
        newTex.Apply();

        return newTex;
    }
  

    IEnumerator CreatePuzzle(RectInt s, int segment_index)
    {
        var temp = new Texture2D(s.xMax - s.xMin + 2, s.yMax - s.yMin + 2, TextureFormat.RGBA32, false);

        temp.SetPixels(1, 1, s.xMax - s.xMin, s.yMax - s.yMin, image.GetPixels(s.xMin, s.yMin, s.xMax - s.xMin, s.yMax - s.yMin));

        for (int i = 0; i < temp.width; i++)
        {
            temp.SetPixel(i, 0, Color.clear);
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
                    temp.SetPixel(i - s.xMin + 1, j - s.yMin + 1, Color.clear);
                }
            }
        }

        temp.Apply();

        puzzle_prefab.GetComponent<PuzzlePiece>().SetIndex(segment_index);
        var sr = puzzle_prefab.GetComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(temp, new Rect(0, 0, temp.width, temp.height), new Vector2(.5f, .5f));
        puzzles.Add(Instantiate(puzzle_prefab,new Vector2(UnityEngine.Random.Range(-6,6), UnityEngine.Random.Range(-4, 4)),Quaternion.identity,gameObject.transform));

        yield return null;
    }

    private RectInt SelectSegment(Texture2D mask, int x, int y, Color c,int number) 
    {
        RectInt res = new RectInt();
        res.xMin = x; res.xMax = x;
        res.yMin = y; res.yMax = y;

        var PixelQueue = new Queue<Tuple<int, int>>();
        PixelQueue.Enqueue(Tuple.Create<int, int>(x, y));


        //procedure which adds pixels at the top or the botom of <x,y> pixel (depends of <offset>) to the queue
        //<was_white> flag is necessary to avoid adding extra pixels to the queue 
        void AddPixelsToQueue(int x, int y, ref bool was_white,int offset)
        {
            if (CloseToWhite(mask.GetPixel(x, y + offset))){
                if (!was_white){        
                    var pixel = Tuple.Create<int, int>(x, y + offset);
                    PixelQueue.Enqueue(pixel);
                    RecalculateBordery(y + offset);
                    was_white = true;                 
                }
            }
            else       
                was_white = false;       
        }
        //procedure which moves to the left or right from <temp> pixel (depends of <offset>)
        //and recolor CloseToWhite pixels into <c> Color. Additionaly marks recolored pixel in
        //<segments_marking> array with <number> index
        void CheckTopAndBottomPixels(Tuple<int, int> temp, ref bool was_white_top, ref bool was_white_bottom, int offset) 
        {
            int shift = offset > 0 ? 1 : 0;
            for (int i = temp.Item1 + shift;i < mask.width && i >= 0; i+= offset){

                if (CloseToWhite(mask.GetPixel(i, temp.Item2)))
                {
                    mask.SetPixel(i, temp.Item2, c);
                    segments_marking[i, temp.Item2] = number;
                }
                else
                {
                    RecalculateBorderx(i-offset);
                    break;
                }
                if(temp.Item2 != 0)
                    AddPixelsToQueue( i, temp.Item2, ref was_white_bottom, -1);
                if (temp.Item2 != mask.height-1)
                    AddPixelsToQueue( i, temp.Item2, ref was_white_top, 1);
            }
        }
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
        //int counter = 0;
        while (PixelQueue.Count != 0)
        {
            bool was_white_top = false;
            bool was_white_bottom = false;
            var temp = PixelQueue.Peek();

            CheckTopAndBottomPixels(temp,ref was_white_top, ref was_white_bottom, (int)Side.Right);
            CheckTopAndBottomPixels(temp, ref was_white_top, ref was_white_bottom, (int)Side.Left);

            PixelQueue.Dequeue();
        }
        mask.Apply();
        return res;
    }

    private bool CloseToWhite(Color c) 
    {
        return c.b + c.g + c.r > 2.8;
    }
}
