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
    [SerializeField] private GameObject ButtonNext;
    [SerializeField] private GameObject ButtonPrev;
    [SerializeField] private int number_of_journals;

    private WaitForFixedUpdate _waitForFixedUpdate = new WaitForFixedUpdate();

    private float _delta_for_1_sec_anim;
    private List<GameObject> Journals;
    private int _cur_journal_page = 0;
    [SerializeField] private int number_of_journal_pages;
    private Vector2[] deff_positions = new Vector2[8];

    private void Start()
    {
        for (int i = 0; i < 4; i++) 
        {
            var temp_u = new Vector2(i * 400 - 600, -500);
            var temp_b = new Vector2(i * 400 - 600, -1000);
            deff_positions[i] = temp_u;
            deff_positions[i+4] = temp_b;
        }

        _delta_for_1_sec_anim = Time.fixedUnscaledDeltaTime;

        Journals = new List<GameObject>();

        for (int i = 0; i < number_of_journals; i++) 
        {
            Journals.Add(Instantiate(Journal, PanelJournalHolder.transform.Find("JournalHolder").transform));
        }

        for (int i = 0; i < number_of_journals; i++)
        {
            var t = Journals[i].GetComponent<RectTransform>();
            t.rotation = Quaternion.Euler(0,0, Random.Range(-15,15));
            t.gameObject.SetActive(false);
        }
        number_of_journal_pages = Mathf.CeilToInt((float)Journals.Count / 8);
    }

    public void OnClickNextPage()
    {
        ChangePage(_cur_journal_page + 1);
    }

    public void OnClickPrevPage()
    {
        ChangePage(_cur_journal_page - 1,false);
    }

    private void ChangePage(int nextPage , bool is_swiping_left = true)
    {
        int startIndex = nextPage * 8;
        int endIndex = startIndex + 8;

        for (int i = _cur_journal_page * 8; i < _cur_journal_page * 8 + 8 && i < number_of_journals; i++)
        {
            StartCoroutine(MoveImage(Vector2.left * ((is_swiping_left ? 1 : -1) * 1000 + Journals[i].GetComponent<RectTransform>().anchoredPosition.x), Journals[i].GetComponent<Image>(), 0.25f, true));
        }

        _cur_journal_page = nextPage;

        for (int i = startIndex; i < endIndex; i++)
        {
            if (i < Journals.Count)
            {
                Journals[i].SetActive(true);
                Vector2 dir = deff_positions[i % 8] - Journals[i].GetComponent<RectTransform>().anchoredPosition;
                StartCoroutine(MoveImage(dir, Journals[i].GetComponent<Image>(), 0.5f));
            }
        }

        ButtonPrev.SetActive(_cur_journal_page > 0);
        ButtonNext.SetActive(_cur_journal_page + 1 < number_of_journal_pages);
    }


    public void OnClickBackToMainMenuButton() 
    {
        ButtonPrev.SetActive(false);
        ButtonNext.SetActive(false);

        StartCoroutine(MoveImage(Vector2.down * 1080, PanelJournalHolder, 0.25f));
        StartCoroutine(MoveImage(Vector2.down * 1080, MainMenuButtonHolder, 0.25f));

        for (int i = _cur_journal_page * 8; i < _cur_journal_page * 8 + 8 && i < number_of_journals; i++)
        {       
            StartCoroutine(MoveImage(-deff_positions[i%8], Journals[i].GetComponent<Image>(), 0.25f,true));
        }
    }

    public void OnClickPlayButton() 
    {
        if (_cur_journal_page != 0)
            ButtonPrev.SetActive(true);

        if (_cur_journal_page + 1 < number_of_journal_pages)
            ButtonNext.SetActive(true);

        StartCoroutine(MoveImage(Vector2.up * 1080, PanelJournalHolder ,0.25f));
        StartCoroutine(MoveImage(Vector2.up * 1080, MainMenuButtonHolder, 0.25f));

        for(int i = _cur_journal_page * 8; i < _cur_journal_page * 8 + 8 && i < number_of_journals; i++) 
        {
            Journals[i].SetActive(true);
            StartCoroutine(MoveImage(deff_positions[i%8], Journals[i].GetComponent<Image>(), 0.5f));
        }
    }

    private IEnumerator MoveImage(Vector2 direction, Image UIelement, float duration, bool deactivate = false) {
        
        float _current_scale = gameObject.GetComponent<RectTransform>().localScale.y;
        float number_of_sim_steps = 50 * duration;
        for (int i = 0; i < number_of_sim_steps; i++)
        {
            UIelement.transform.position += (Vector3)direction * _current_scale * _delta_for_1_sec_anim * (1 / duration);
            yield return _waitForFixedUpdate;
        }

        if (deactivate)
            UIelement.gameObject.SetActive(false);
    }



    

    

}
