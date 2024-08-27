using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class PuzzleDataTracker : MonoBehaviour
{
    private int number_of_puzzles_in_scene;
    private int current_max_of_combined_puzzles = 1;
    private Material image_mat;
    [SerializeField] private bool interaction_disabled = false;

    public bool IsInteractionDisabled() => interaction_disabled;
    public void SetInteractionBool(bool set) 
    {
        interaction_disabled = set;
    }
    private void Awake()
    {
        image_mat = GetComponent<SpriteRenderer>().material;
        image_mat.SetFloat("_StartAnim", 0);
        image_mat.SetFloat("_CurAnimSecond", 0);
    }

    public void SetMaxComb(int n) 
    {
        if(n > current_max_of_combined_puzzles) 
        {
            current_max_of_combined_puzzles = n;
            if(current_max_of_combined_puzzles == number_of_puzzles_in_scene) 
            {
                StartCoroutine(GameEnd());
            }

        }
        
    }

    public void SetNumberOfPuzzles(int n) 
    {
        number_of_puzzles_in_scene = n;
    }

    private IEnumerator GameEnd() 
    {
        interaction_disabled = true;
        var sr = GetComponent<SpriteRenderer>();
        var pg = GetComponent<PuzzleGeneration>();
        var image = pg.GetImage();


        sr.sprite = Sprite.Create(image, new Rect(1, 1, image.width-1, image.height-1), new Vector2(.5f, .5f));
        transform.position = pg.GetCenter();
        sr.enabled = true;

        pg.DisableShader();

        for (float i = 0; i < 1.1f;) 
        {
            image_mat.SetFloat("_CurAnimSecond", i);
            i += 0.02f;
            yield return new WaitForFixedUpdate();
        }
        
        pg.DisablePuzzles();
        pg.EnableShader();
        interaction_disabled = false;
        print("game end");
        yield return null;
    }

}



//void mainImage(out vec4 fragColor, in vec2 fragCoord)
//{
//    // Normalized pixel coordinates (from 0 to 1)
//    vec2 uv = fragCoord / iResolution.xy * 2.0 - 1.0;

//    // Time varying pixel color
//    float d = length(uv);

//    d -= log(iTime + 1.0);
//    //d = abs(d);

//    d = smoothstep(0.015, 0.03, d);

//    // Output to screen
//    fragColor = vec4(1.0 - d, 1.0 - d, 1.0 - d, 1);
//}