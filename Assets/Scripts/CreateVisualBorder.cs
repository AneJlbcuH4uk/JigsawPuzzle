using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateVisualBorder : MonoBehaviour
{
    private Vector2 border_limit;
    private GameObject holder;
    [SerializeField] int border_width = 100;
    [SerializeField] Color border_color = Color.black;

    void Start()
    {
        border_limit = InputControl.instance.GetBorderlimit();
        
        holder = new GameObject("borderholder");

        var texture = new Texture2D(border_width, border_width);
        var colors = new Color[border_width * border_width];
        Array.Fill(colors, Color.white);
        texture.SetPixels(colors);
        texture.Apply();

        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f));
        

        GameObject child = new GameObject("framepart");
        child.transform.SetParent(holder.transform);
        var sr = child.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = border_color;

        child.transform.localScale = new Vector3( 200 / border_width * border_limit.x + 1, 1, 1);

        Instantiate(child, new Vector3(0, border_limit.y, 19), Quaternion.identity, holder.transform);
        Instantiate(child, new Vector3(0, -border_limit.y, 19),Quaternion.identity, holder.transform);
  
        child.transform.localScale = new Vector3(1, 200 / border_width * border_limit.y + 1, 1);

        Instantiate(child, new Vector3(border_limit.x, 0, 19), Quaternion.identity, holder.transform);
        Instantiate(child, new Vector3(-border_limit.x, 0, 19), Quaternion.identity, holder.transform);

        Destroy(child);

    }

}
