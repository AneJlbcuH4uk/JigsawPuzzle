using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;




public class UIBehaviour : MonoBehaviour
{
    [SerializeField] private Image PanelJournalHolder;
    [SerializeField] private Image MainMenuButtonHolder;
    [SerializeField] private GameObject Journal;

    private WaitForFixedUpdate _waitForFixedUpdate = new WaitForFixedUpdate();

    private float _delta_for_1_sec_anim;
    private List<GameObject> Journals;

    private Vector2[] deff_positions = new Vector2[8];

    private void Start()
    {
        for(int i = 0; i < 4; i++) 
        {
            var temp_u = new Vector2(i * 400 - 600, -500);
            var temp_b = new Vector2(i * 400 - 600, -1000);
            deff_positions[i] = temp_u;
            deff_positions[i+4] = temp_b;
        }

        _delta_for_1_sec_anim = Time.fixedUnscaledDeltaTime;

        Journals = new List<GameObject>();

        for (int i = 0; i < 8; i++) 
        {
            Journals.Add(Instantiate(Journal, PanelJournalHolder.transform));
        }
        for (int i = 0; i < 8; i++)
        {
            var t = Journals[i].GetComponent<RectTransform>();
            //t.anchoredPosition = deff_positions[i];
            t.rotation = Quaternion.Euler(0,0, Random.Range(-20,20));
        }

    }

    public void OnClickBackToMainMenuButton() 
    {
        StartCoroutine(MoveImage(Vector2.down * 1080, PanelJournalHolder, 0.25f));
        StartCoroutine(MoveImage(Vector2.down * 1080, MainMenuButtonHolder, 0.25f));

        for (int i = 0; i < 8; i++)
        {
            StartCoroutine(MoveImage(-deff_positions[i], Journals[i].GetComponent<Image>(), 0.25f));
        }
    }

    public void OnClickPlayButton() 
    {
        StartCoroutine(MoveImage(Vector2.up * 1080, PanelJournalHolder ,0.25f));
        StartCoroutine(MoveImage(Vector2.up * 1080, MainMenuButtonHolder, 0.25f));

        for(int i = 0; i < 8; i++) 
        {
            StartCoroutine(MoveImage(deff_positions[i], Journals[i].GetComponent<Image>(), 0.5f));
        }
    }

    private IEnumerator MoveImage(Vector2 direction, Image UIelement, float duration) {
        
        float _current_scale = gameObject.GetComponent<RectTransform>().localScale.y;
        float number_of_sim_steps = 50 * duration;
        for (int i = 0; i <= number_of_sim_steps; i++)
        {
            UIelement.transform.position += (Vector3)direction * _current_scale * _delta_for_1_sec_anim * (1 / duration);
            yield return _waitForFixedUpdate;
        }
    }



    

    

}
