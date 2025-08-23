using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class JournalDecalGen : MonoBehaviour
{
    [SerializeField] Vector2 image_rect = new Vector2 (200f,200f);
    [SerializeField] Vector2 centers_location_rect = new Vector2 (100f,100f);
    //[SerializeField] int number_of_images_in_decal = 5;

    string path_to_images = Application.streamingAssetsPath + "/Puzzles/TestPuzzle1";
    string path_to_decals = Application.streamingAssetsPath + "/Decals";

    [SerializeField] private List<Texture2D> decal_images = new List<Texture2D>();
    [SerializeField] private Texture2D image_placeholder;

    private float[] divide_coefs = new float[]{0, 1, 4, 10, 4, 1};
    private List<Vector2> centers;
    [SerializeField] private Texture2D decal;
    [SerializeField] private Color image_border_colors;
    [SerializeField] private float boarder_thickness_percent = 0.01f;

    private bool images_loaded = false;
    private bool decal_created = false;

    public bool Is_Decal_created() => decal_created;


    public void SetRect(Vector2 vec) 
    {
        centers_location_rect = vec;
    }

    
    public IEnumerator LoadDecal() 
    {
        var info = new DirectoryInfo(Path.GetDirectoryName(path_to_images));
        var decals = info.GetFiles();

        try
        {
            path_to_images = transform.parent.GetComponent<UIJournalData>().GetPath();
        }
        catch
        {
            Debug.LogError($"Failed to get path");
        }

        //print(path_to_decals + "/" + Path.GetFileName(path_to_images));
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file://" + path_to_decals + "/" + Path.GetFileName(path_to_images)))
        {
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D tempTexture = DownloadHandlerTexture.GetContent(uwr);
                decal = tempTexture;
                SetDecalFromTexture();
                yield break;
            }
            else 
            {
                Debug.Log("Failed to load texture generating a new one");
            }
            
        }






        Normalize_coefs();
        centers = GenerateCenters();
        Vector2 max_image_size = (image_rect - centers_location_rect) / Mathf.Sqrt(2);

        

        //print(gameObject.GetComponent<Image>().IsActive());
        StartCoroutine(LoadImages(max_image_size));
        yield return new WaitUntil(() => images_loaded);
        CreateDecal();
        //yield return null;
        
    } 


    private Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        RenderTexture.active = rt;

        Graphics.Blit(source, rt);

        Texture2D result = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);
        result.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        result.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return result;
    }
    private void CreateDecal() 
    {
        decal = new Texture2D((int)image_rect.x, (int)image_rect.y);
        decal.wrapMode = TextureWrapMode.Clamp;
        for (int i = 0; i < image_rect.x; i++)
            for (int j = 0; j < image_rect.y; j++)
                decal.SetPixel(i, j, Color.clear);


        Vector2 center_of_decal = image_rect / 2;
        Vector2 corner_of_loc_rect = center_of_decal - centers_location_rect / 2;

        for(int i = 0; i < centers.Count;i++) 
        {
            float angle = Random.Range(-35f, 35f);
            float angleRad = Mathf.Deg2Rad * angle;

            float cos = Mathf.Cos(angleRad);
            float sin = Mathf.Sin(angleRad);

            Vector2 center = centers[i] + corner_of_loc_rect;
            Vector2 corner = center - new Vector2(decal_images[i].width, decal_images[i].height) / 2;

            Vector2 pivot = new Vector2(decal_images[i].width, decal_images[i].height) / 2f;

            for(float y = 0; y < decal_images[i].height; y += 0.5f) 
            {
                for(float x = 0; x < decal_images[i].width; x += 0.5f) 
                {
                    Vector2 coord = new Vector2(x, y) - pivot;
                    int NewX = Mathf.RoundToInt(coord.x * cos - coord.y * sin + pivot.x + corner.x);
                    int NewY = Mathf.RoundToInt(coord.x * sin + coord.y * cos + pivot.y + corner.y);

                    //if(decal.GetPixel(NewX,NewY) == Color.clear)
                    decal.SetPixel(NewX, NewY, decal_images[i].GetPixel(Mathf.RoundToInt(x), Mathf.RoundToInt(y)));

                }
            }


            //print("corner  = " + corner + " | rect = " + decal_images[i].width + " | " + decal_images[i].height + " | " + center);
            //decal.SetPixels((int)corner.x, (int)corner.y, decal_images[i].width, decal_images[i].height, decal_images[i].GetPixels());

            
        }

        decal.Apply();
        byte[] itemBGBytes = decal.EncodeToPNG();
        //print(Path.GetFileNameWithoutExtension(path_to_images));
        File.WriteAllBytes( path_to_decals + $"/{Path.GetFileNameWithoutExtension(path_to_images)}", itemBGBytes);
        SetDecalFromTexture();
        //print(im.textureRect);
    }

    private void SetDecalFromTexture() 
    {
        GetComponent<Image>().sprite = Sprite.Create(decal, new Rect(0, 0, decal.width, decal.height), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);
        decal_created = true;
    }


    private IEnumerator LoadImages(Vector2 max_image_size) 
    {
        var info = new DirectoryInfo(path_to_images);
        var fileInfo = info.GetFiles();
        float ratio;
        Vector2 new_rect = Vector2.zero;
        foreach (var v in fileInfo) 
        {
            
            if (JournalData._is_file_image(v.Name))
            {
                
                using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file://" + v.FullName))
                {
                    yield return uwr.SendWebRequest();
                    if (uwr.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"Failed to load texture: {uwr.error}");
                        yield break;
                    }
                    if(decal_images.Count > 5) 
                    {
                        break;
                    }
                    

                    Texture2D tempTexture = DownloadHandlerTexture.GetContent(uwr);
                    ratio = (float)tempTexture.height / tempTexture.width;

                    //print(max_image_size);
                    new_rect.x = ratio >= 1 ? max_image_size.x : max_image_size.x * ratio;
                    new_rect.y = ratio > 1 ? max_image_size.y / ratio : max_image_size.y;

                    for (int i = 0; i < (ratio <= 1 ? tempTexture.height * boarder_thickness_percent : tempTexture.width * boarder_thickness_percent); i++) 
                    {
                        for(int j = 0; j < tempTexture.height; j++) 
                        {
                            tempTexture.SetPixel(i, j, image_border_colors);
                            tempTexture.SetPixel(tempTexture.width - 1 - i, j, image_border_colors);
                        }
                        for (int j = 0; j < tempTexture.width; j++)
                        {
                            tempTexture.SetPixel(j, i, image_border_colors);
                            tempTexture.SetPixel(j, tempTexture.height - 1 - i, image_border_colors);
                        }
                    }
                    tempTexture.Apply();

                    //print(tempTexture.height + " | " + tempTexture.width);
                    decal_images.Add(ResizeTexture(tempTexture, (int)new_rect.y, (int)new_rect.x));
                    //print(new_rect);
                    
                }
            }
        }
        ratio = (float)image_placeholder.height / (float)image_placeholder.width;
        new_rect.x = max_image_size.x * ratio;
        new_rect.y = max_image_size.y;

        for (int i = decal_images.Count; i < 5; i++) 
        {
            decal_images.Add(ResizeTexture(image_placeholder, (int)new_rect.y + (4 - (int)new_rect.y%4), (int)new_rect.x + (4 - (int)new_rect.x % 4)));
        }

        images_loaded = true;
        print("images for decal loaded");
    }


    private void Normalize_coefs() 
    {
        float sum = divide_coefs.Sum();
        float part_sum = 0;
        for(int i = 0; i < divide_coefs.Length; i++) 
        {
            part_sum += divide_coefs[i];
            divide_coefs[i] = part_sum/sum;
        }
    }

    private List<Vector2> GenerateCenters() 
    {
        Vector2 Vector_Calc(int index_x, int index_y) 
        {
            return new Vector2(divide_coefs[index_x] * centers_location_rect.x + (divide_coefs[index_x + 1] - divide_coefs[index_x]) * centers_location_rect.x / 2,
                                           divide_coefs[index_y] * centers_location_rect.y + (divide_coefs[index_y + 1] - divide_coefs[index_y]) * centers_location_rect.y / 2);
        }

        List<int> rows = new List<int>() { 0, 1, 2, 3, 4 };
        List<int> cols = new List<int>() { 0, 1, 2, 3, 4 };

        List<Vector2> selected_tiles = new List<Vector2>();

        for(int i = 4; i >= 0; i--) 
        {
            int r = Random.Range(0, i + 1);
            int c = Random.Range(0, i + 1);

            selected_tiles.Add(Vector_Calc(rows[r], cols[c]));

            rows.RemoveAt(r);
            cols.RemoveAt(c);
        }
        return selected_tiles;
    }








   
}
