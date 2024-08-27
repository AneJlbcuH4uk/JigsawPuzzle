using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class JournalData
{
    private List<PuzzleData> puzzles;
    private string path = Application.streamingAssetsPath + "/Puzzles/TestPuzzle1";
    private static int items_per_page = 3;
    private int number_of_pages;

    public JournalData(string path)
    {
        this.path = path;
        this.puzzles = new List<PuzzleData>();
        InitializePuzzles();
    }

    private void InitializePuzzles()
    {
        var info = new DirectoryInfo(path);
        var fileInfo = info.GetFiles();

        foreach (var file in fileInfo)
        {
            if (file.Name[^5..] != ".meta")
            {
                var temp = new PuzzleData(Path.Combine(path, file.Name), MaskType.Classic, 4, 5);
                puzzles.Add(temp);
            }
        }
        number_of_pages = Mathf.CeilToInt((float)puzzles.Count / items_per_page);
    }

    public PuzzleData[] GetPage(int page) 
    {
        PuzzleData[] res = new PuzzleData[items_per_page];

        for (int i = 0; i < items_per_page; i++)
        {
            if (i + page * items_per_page < puzzles.Count)
                res[i] = puzzles[i + page * items_per_page];
            else
                res[i] = null;
        }
        return res;
    }

    public static int GetItemsPerPage() => items_per_page;
    public int GetNumberOfPages() => number_of_pages;

}


public class PuzzleData
{
    string image;
    MaskType mt;
    int num;
    int off;

    public PuzzleData(string s, MaskType m, int n, int o)
    {
        Image = s;
        Mt = m;
        Num = n;
        Off = o;
    }

    public string Image { get => image; set => image = value; }
    public MaskType Mt { get => mt; set => mt = value; }
    public int Num { get => num; set => num = value; }
    public int Off { get => off; set => off = value; }
}
