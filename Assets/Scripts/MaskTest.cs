using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaskTest : MonoBehaviour
{
    [SerializeField] Vector2Int lol = new Vector2Int(1920,1080);
    [SerializeField] int cock = 5; // for deffault res not bigger than 36
    void Start()
    {
        var mg = new MaskGenerator(lol, cock, MaskType.Hex);
        var t = gameObject.GetComponent<Image>();
        t.sprite = Sprite.Create(mg.GetMask(), new Rect(0, 0, lol.x, lol.y),new Vector2(.5f, .5f));
        t.SetNativeSize();
    }

}
