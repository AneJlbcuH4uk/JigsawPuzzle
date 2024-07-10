using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;




public class UIBehaviour : MonoBehaviour
{
    [SerializeField] private Image PanelJournalHolder;
    [SerializeField] private Image MainMenuButtonHolder;
    [SerializeField] private GameObject Journal_pref;
    [SerializeField] private GameObject ButtonNext;
    [SerializeField] private GameObject ButtonPrev;
    [SerializeField] private GameObject ButtonClose;
    [SerializeField] private int number_of_journals;

    private WaitForFixedUpdate _waitForFixedUpdate = new WaitForFixedUpdate();

    private float _delta_for_1_sec_anim;
    private List<GameObject> Journals;
    private int _cur_journal_page = 0;
    private int number_of_journal_pages;
    private Vector2[] deff_positions = new Vector2[8];
    [SerializeField] private GameObject Opened_Journal = null;


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
            Journals.Add(Instantiate(Journal_pref, PanelJournalHolder.transform.Find("JournalHolder").transform));
        }

        for (int i = 0; i < number_of_journals; i++)
        {
            var b = Journals[i].GetComponent<Button>();
            b.onClick.AddListener(OnClickJournalButton);

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
                Journals[i].GetComponent<UIJournalData>().ClearData();
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
            StartCoroutine(MoveImage(-Journals[i].GetComponent<RectTransform>().anchoredPosition, Journals[i].GetComponent<Image>(), 0.25f,true));
            StartCoroutine(RotateImage(Journals[i].GetComponent<UIJournalData>().GetAngle(), Journals[i].GetComponent<Image>(), 0.5f));
            Journals[i].GetComponent<UIJournalData>().ClearData();
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
            Journals[i].GetComponent<UIJournalData>().ClearData();
        }
    }

    private IEnumerator MoveImage(Vector2 direction, Image UIelement, float duration, bool deactivate = false) {

        //print("move to " + direction);
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

    private IEnumerator RotateImage(float rotation, Image UIelement, float duration, bool deactivate = false) 
    {     
        float number_of_sim_steps = 50 * duration;
        //print("trying rotate journal by " + rotation + " degrees");
        for (int i = 0; i < number_of_sim_steps; i++)
        {
            //print(Quaternion.Euler(0, 0, rotation * _delta_for_1_sec_anim * (1 / duration)));
            UIelement.gameObject.GetComponent<RectTransform>().rotation *= Quaternion.AngleAxis( rotation * _delta_for_1_sec_anim * (1 / duration), Vector3.forward);
            yield return _waitForFixedUpdate;
        }

        if (deactivate)
            UIelement.gameObject.SetActive(false);
    }
    

    public void OnClickJournalButton() 
    {
        ButtonClose.SetActive(true);
        Opened_Journal = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        var jd = Opened_Journal.GetComponent<UIJournalData>();
        jd.UpdateData();

        //Opened_Journal.transform.position += Vector3.forward;
        Vector2 centerOfScreen = new Vector2(0, -1080)/2;
        StartCoroutine(MoveImage(centerOfScreen - Opened_Journal.GetComponent<RectTransform>().anchoredPosition, Opened_Journal.GetComponent<Image>(), 0.2f));

        float angle = - Opened_Journal.GetComponent<RectTransform>().rotation.eulerAngles.z;
        angle = Mathf.Abs(angle) > 180 ? angle = 360 + angle : angle;
        StartCoroutine(RotateImage(angle, Opened_Journal.GetComponent<Image>(), 0.25f));

    }

    public void OnClickButtonClose() 
    {
        ButtonClose.SetActive(false);
        var jd = Opened_Journal.GetComponent<UIJournalData>();
        StartCoroutine(MoveImage(jd.GetPos() - Opened_Journal.GetComponent<RectTransform>().anchoredPosition, Opened_Journal.GetComponent<Image>(), 0.2f));
        StartCoroutine(RotateImage(jd.GetAngle(), Opened_Journal.GetComponent<Image>(), 0.25f));

        //jd.ClearData();
        Opened_Journal = null;         
        //Opened_Journal.transform.position += Vector3.back;

    }

    


    

}
