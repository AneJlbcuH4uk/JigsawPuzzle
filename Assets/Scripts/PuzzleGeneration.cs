using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;
using UnityEngine.UIElements;

enum Side: int
{
    Left = -1,
    Right = 1
}


public class PuzzleGeneration : MonoBehaviour
{
    [SerializeField] private Texture2D mask;
    [SerializeField] private Texture2D image;
    [SerializeField] private GameObject puzzle_prefab;
    [SerializeField] private List<GameObject> puzzles;
    [SerializeField] private int offset;

    private Vector2 image_offset = new Vector2 (6,3);
    private Texture2D mask_copy;
    private int[,] segments_marking;
    private List<RectInt> puzzle_shapes;
 
    private void Start()
    {
        int segement_number = 1;
        int number_of_segments_to_generate = 3;

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
                    
                    //if (segement_number > number_of_segments_to_generate) goto end_of_cycle;                 
                }
            }
        }
        //end_of_cycle:
        //print();
        for (int j = 1; j < mask_copy.height; j += offset)
        {
            StartCoroutine(CheckLine(j, mask_copy.width,true));
        }

        for (int j = 1; j < mask_copy.width; j += offset)
        {
            StartCoroutine(CheckLine(j, mask_copy.height,false));
        }


        print("generation time = " + Time.realtimeSinceStartup);

        ShufflePuzzles();

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

    private void AddPair(int puzzle_index_1, int puzzle_index_2)
    {
        var data_1 = puzzles[puzzle_index_1 - 1].GetComponent<PuzzlePiece>();
        var data_2 = puzzles[puzzle_index_2 - 1].GetComponent<PuzzlePiece>();

        data_1.Add_neighbour(puzzle_index_2, data_2.GetCenter(), data_2.GetCollider());
        data_2.Add_neighbour(puzzle_index_1, data_1.GetCenter(), data_1.GetCollider());
    }

    private void ShufflePuzzles() 
    {
        foreach (var t in puzzles)
        {
            Vector2 random_pos = new Vector2(UnityEngine.Random.Range(0, 6), UnityEngine.Random.Range(-2, 2));
            t.GetComponent<PuzzlePiece>().MovePuzzle(random_pos);
        }
    }


    IEnumerator CheckLine(int index, int length, bool isRow)
    {
        //yield return new WaitForSeconds(1);
        int last_index = 0;
        bool was_zero = false;
        for (int i = 0; i < length; i++)
        {
            int value = isRow ? segments_marking[i, index] : segments_marking[index, i];
            if (value != 0)
            {
                if (was_zero)
                {
                    if (last_index != 0 && last_index != value)
                    {
                        AddPair(last_index, value);
                    }
                    last_index = value;
                    was_zero = false;
                }
            }
            else
            {
                was_zero = true;
            }
        }
        yield return null;
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

        Vector2 place = new Vector2(s.xMax + s.xMin, s.yMax + s.yMin) / 200 - image_offset;

        

        var obj = Instantiate(puzzle_prefab, place, Quaternion.identity, gameObject.transform);
        puzzles.Add(obj);


        var pp = obj.GetComponent<PuzzlePiece>();
        var sr = obj.GetComponent<SpriteRenderer>();

        pp.SetIndex(segment_index);
        pp.SetCenter(place);
        sr.sprite = Sprite.Create(temp, new Rect(0, 0, temp.width, temp.height), new Vector2(.5f, .5f));

        StartCoroutine(UpdateShapeToSprite(obj));

        yield return null;
    }


    private IEnumerator UpdateShapeToSprite(GameObject obj)
    {
        Destroy(obj.GetComponent<PolygonCollider2D>(), 1);
        var t = obj.AddComponent<PolygonCollider2D>();
        obj.GetComponent<PuzzlePiece>().Set_collider(t);
        yield return null;
    }

    private RectInt SelectSegment(Texture2D mask, int x, int y, Color c,int number) 
    {
        RectInt res = new RectInt();
        res.xMin = x; res.xMax = x;
        res.yMin = y; res.yMax = y;

        var PixelQueue = new Queue<Tuple<int, int>>();
        PixelQueue.Enqueue(Tuple.Create<int, int>(x, y));

        bool CloseToWhite(Color c)
        {
            return c.b + c.g + c.r > 2.8;
        }
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

    
}
