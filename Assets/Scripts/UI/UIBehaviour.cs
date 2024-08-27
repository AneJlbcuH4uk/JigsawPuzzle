
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;




public class UIBehaviour : MonoBehaviour
{
    [SerializeField] private Image PanelJournalHolder;
    [SerializeField] private Image MainMenuButtonHolder;
    [SerializeField] private GameObject Journal_pref;
    [SerializeField] private GameObject ButtonNext;
    [SerializeField] private GameObject ButtonPrev;
    [SerializeField] private GameObject ButtonClose;   
    [SerializeField] private GameObject BackGround;
    [SerializeField] private GameObject LoadingScreen;
    [SerializeField] private GameObject SettingsMenu;
    [SerializeField] private GameObject ButtonSettings;
    [SerializeField] private GameObject BackToMainMenuButton;

    private GameObject Opened_Journal = null;

    //references for journalUI 
    [SerializeField] private GameObject JournalUI;
    private GameObject RightPage;
    private GameObject LeftPage;
    
    // list of paths to image directories
    [SerializeField] private List<string> puzzle_sets;


    private int number_of_journals;
    private float _delta_for_1_sec_anim;
    private List<GameObject> Journals; 
    private int number_of_journal_pages;

    private WaitForFixedUpdate _waitForFixedUpdate = new WaitForFixedUpdate();
    private int _cur_journal_page = 0;
    private string path_to_set_folders = Application.streamingAssetsPath + "/Puzzles"; // probably better use Recources 
    private Vector2[] deff_positions = new Vector2[8];

    private Vector2 old_pos_of_settings_button;

    // general method used to move image over time
    // Vector2 direction => coordinates of point towards which object gona move
    // Image UIelement => UIElement which is moved
    // float duration => duration in seconds how long to move object towards point
    // bool deactivate => set true if gona setactive(false) at the end of movement
    private IEnumerator MoveImage(Vector2 direction, Image UIelement, float duration, bool deactivate = false)
    {

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


    // general method used to rotate image over time
    // float rotation => angle in degrees to which object gona rotate
    // Image UIelement => UIElement which is rotated
    // float duration => duration in seconds how long to rotated object towards angle
    // bool deactivate => set true if gona setactive(false) at the end of movement
    private IEnumerator RotateImage(float rotation, Image UIelement, float duration, bool deactivate = false)
    {
        float number_of_sim_steps = 50 * duration;
        //print("trying rotate journal by " + rotation + " degrees");
        for (int i = 0; i < number_of_sim_steps; i++)
        {
            //print(Quaternion.Euler(0, 0, rotation * _delta_for_1_sec_anim * (1 / duration)));
            UIelement.gameObject.GetComponent<RectTransform>().rotation *= Quaternion.AngleAxis(rotation * _delta_for_1_sec_anim * (1 / duration), Vector3.forward);
            yield return _waitForFixedUpdate;
        }

        if (deactivate)
            UIelement.gameObject.SetActive(false);
    }

    private void Start()
    {
        LoadingScreen.transform.GetChild(1).GetComponent<Image>().material.SetFloat("_CurFillPercent", 0.5f);

        puzzle_sets = Directory.GetDirectories(path_to_set_folders).ToList<string>();
        number_of_journals = puzzle_sets.Count;

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

            Journals[i].GetComponent<UIJournalData>().SetImagesPath(puzzle_sets[i]);
        }
        number_of_journal_pages = Mathf.CeilToInt((float)Journals.Count / 8);

        RightPage = JournalUI.transform.GetChild(2).gameObject;
        LeftPage = JournalUI.transform.GetChild(1).gameObject;

        old_pos_of_settings_button = ButtonSettings.GetComponent<RectTransform>().anchoredPosition;

    }
    
    // __________________________________________________________________________________________
    // logic of lower half of UI regarding Journals and their movement  

    // called when next page pressed
    public void OnClickNextPage()
    {
        ChangePage(_cur_journal_page + 1);
    }

    // called when prev page pressed
    public void OnClickPrevPage()
    {
        ChangePage(_cur_journal_page - 1,false);
    }

    // moves journals to left or right depends on int nextPage to move right pass _cur_journal_page + 1 else _cur_journal_page - 1
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

    //scrol menu back to main (upper half) hides instantiated journals
    public void OnClickBackToMainMenuButton() 
    {
        ButtonPrev.SetActive(false);
        ButtonNext.SetActive(false);

        StartCoroutine(MoveImage(Vector2.down * 1080, PanelJournalHolder, 0.25f));
        StartCoroutine(MoveImage(Vector2.down * 1080, MainMenuButtonHolder, 0.25f));
        StartCoroutine(MoveBackGround(Vector2.down * 1080, 0.25f));
        for (int i = _cur_journal_page * 8; i < _cur_journal_page * 8 + 8 && i < number_of_journals; i++)
        {
            StartCoroutine(MoveImage(-Journals[i].GetComponent<RectTransform>().anchoredPosition, Journals[i].GetComponent<Image>(), 0.25f,true));
            //StartCoroutine(RotateImage(Journals[i].GetComponent<UIJournalData>().GetAngle(), Journals[i].GetComponent<Image>(), 0.5f));
            Journals[i].GetComponent<UIJournalData>().ClearData();
        }
    }

    // scroll menu to the bottom instantiate journals and arrange them according to deff_positions
    public void OnClickPlayButton() 
    {


        if (_cur_journal_page != 0)
            ButtonPrev.SetActive(true);

        if (_cur_journal_page + 1 < number_of_journal_pages)
            ButtonNext.SetActive(true);

        float anim_delay = 0.25f;

        StartCoroutine(MoveImage(Vector2.up * 1080, PanelJournalHolder , anim_delay));
        StartCoroutine(MoveImage(Vector2.up * 1080, MainMenuButtonHolder, anim_delay));
        StartCoroutine(MoveBackGround(Vector2.up * 1080, anim_delay));

        StartCoroutine(ChangeStateWithDelay(MainMenuButtonHolder.gameObject, anim_delay, false));
        StartCoroutine(ChangeStateWithDelay(BackToMainMenuButton, anim_delay * 2, true));

        for(int i = _cur_journal_page * 8; i < _cur_journal_page * 8 + 8 && i < number_of_journals; i++) 
        {
            Journals[i].SetActive(true);
            StartCoroutine(MoveImage(deff_positions[i%8], Journals[i].GetComponent<Image>(), anim_delay * 2));
            //StartCoroutine(RotateImage(-Journals[i].GetComponent<UIJournalData>().GetAngle(), Journals[i].GetComponent<Image>(), 0.5f));
            Journals[i].GetComponent<UIJournalData>().ClearData();
        }
    }

    // __________________________________________________________________________________________

    private IEnumerator MoveBackGround(Vector2 dir, float duration) 
    {
        float _current_scale = gameObject.GetComponent<RectTransform>().localScale.y;
        float number_of_sim_steps = 50 * duration;
        for (int i = 0; i < number_of_sim_steps; i++)
        {
            BackGround.transform.position += (Vector3)dir * _current_scale * _delta_for_1_sec_anim * (1 / duration) / 100;
            yield return _waitForFixedUpdate;
        }
    }


    // __________________________________________________________________________________________
    // logic of JournalUI
    private int cur_page = 0;
    public void OnClickJournalButton() 
    {
        ButtonClose.SetActive(true);
        Opened_Journal = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        var jd = Opened_Journal.GetComponent<UIJournalData>();
        jd.UpdateData();

        // move journal to center of screen
        Vector2 centerOfScreen = new Vector2(0, -1080)/2;
        StartCoroutine(MoveImage(centerOfScreen - Opened_Journal.GetComponent<RectTransform>().anchoredPosition, Opened_Journal.GetComponent<Image>(), 0.2f));

        // rotate journal to default state 
        float angle = -Opened_Journal.GetComponent<RectTransform>().rotation.eulerAngles.z;
        angle = Mathf.Abs(angle) > 180 ? angle = 360 + angle : angle;
        StartCoroutine(RotateImage(angle, Opened_Journal.GetComponent<Image>(), 0.25f));

        // add opening animation
        JournalUI.SetActive(true);

        CheckRightFlipButton(jd.GetNumberOfPages(), cur_page);
        CheckLeftFlipButton(jd.GetNumberOfPages(), cur_page);
        DrawImagesOnPage(cur_page, jd);
    }

    // activates RightPage if there is images that need to be drawn on, else deactivates
    // activates button to flip page if there is a page, else deactivates
    private void CheckRightFlipButton(int number_of_p, int page)
    {
        // page
        if (page < number_of_p)
            RightPage.SetActive(true);
        else
            RightPage.SetActive(false);

        // flip button
        if (number_of_p > 1 && page + 1 != number_of_p)
        {
            RightPage.transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            RightPage.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    // activates LeftPage if there is images that need to be drawn on, else deactivates
    // activates button to flip page if there is a page, else deactivates
    private void CheckLeftFlipButton(int number_of_p, int page)
    {
        // page
        if (page > 0)
            LeftPage.SetActive(true);
        else
            LeftPage.SetActive(false);

        // flip button
        if (number_of_p > 1 && page != 0)
        {
            LeftPage.transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            LeftPage.transform.GetChild(0).gameObject.SetActive(false);
        }
    }


    // called when flip next button is pressed
    public void FlipNext() 
    {
        //add flip animation
        DrawPage(1);
        DrawPage(1);
    }

    // called when flip prev button is pressed
    public void FlipPrev()
    {
        //add flip animation
        if (cur_page % 2 == 0)
            DrawPage(-2);
        else 
            DrawPage(-1);

        if(cur_page > 0)
            DrawPage(-1);
    }

    // draws page for JouranlUI checks if additional pages / buttons need to be active
    private void DrawPage(int incr)
    {
        cur_page += incr;
        var jd = Opened_Journal.GetComponent<UIJournalData>();

        CheckRightFlipButton(jd.GetNumberOfPages(), cur_page);
        CheckLeftFlipButton(jd.GetNumberOfPages(), cur_page);

        DrawImagesOnPage(cur_page, jd);
    }

    // draws images on a page for JouranlUI depends on int page(number).
    // For journal spread requires 2 calls 1 for left and 1 for right.
    private void DrawImagesOnPage(int page, UIJournalData jd) 
    {
        Transform pagepart = page % 2 == 0 ? RightPage.transform : LeftPage.transform;

        if (page >= 0 && jd.GetNumberOfPages() > page)
        {
            var data = jd.GetPage(page);           
            for (int i = 0; i < JournalData.GetItemsPerPage(); i++)
            {
                var photo = pagepart.GetChild(i + 1);              
                if (data[i] != null)
                {
                    photo.gameObject.SetActive(true);
                    StartCoroutine( photo.GetComponent<UIPuzzleData>().SetUIPuzzleData(data[i]));
                }
                else
                {
                    photo.gameObject.SetActive(false);
                }
            }
        }
    }

    // closes JouranlUI
    public void OnClickButtonClose() 
    {
        ButtonClose.SetActive(false);
        var jd = Opened_Journal.GetComponent<UIJournalData>();
        StartCoroutine(MoveImage(jd.GetPos() - Opened_Journal.GetComponent<RectTransform>().anchoredPosition, Opened_Journal.GetComponent<Image>(), 0.2f));
        StartCoroutine(RotateImage(jd.GetAngle(), Opened_Journal.GetComponent<Image>(), 0.25f));

        //jd.ClearData();
        Opened_Journal = null;
        //Opened_Journal.transform.position += Vector3.back;

        cur_page = 0;

        // add closing animation
        JournalUI.SetActive(false);
    }

    // Starts selected puzzle with set parameters on click on choosen image
    public void OnClickStartPuzzleButton() 
    {
        var data = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<UIPuzzleData>();
        PuzzleLoadingData.SetData(data);
        StartCoroutine(LoadSceneAsync(PuzzleLoadingData.scene_name));
        
    }

    IEnumerator LoadSceneAsync(string name) 
    {
        yield return null;
        var im = LoadingScreen.transform.GetChild(1).GetComponent<Image>();
        im.material.SetFloat("_CurFillPercent", 0);

        im.sprite = Sprite.Create(PuzzleLoadingData.GetTexture(), new Rect(0, 0, PuzzleLoadingData.GetTexture().width, PuzzleLoadingData.GetTexture().height), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect);


        im.rectTransform.sizeDelta = PuzzleLoadingData.GetImageMeasures();

        AsyncOperation op = SceneManager.LoadSceneAsync(name, LoadSceneMode.Single);
        LoadingScreen.SetActive(true);
        
        while (!op.isDone) 
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f) / 10;
            //print(progress);
            im.material.SetFloat("_CurFillPercent", progress);
            yield return null;
        }
        
    }

    // __________________________________________________________________________________________

    Vector2 center_coord_for_settings_button = new Vector2(1920,1080)/-2 ;
    
    public void OpenSettingsMenu() 
    {
        if (!SettingsMenu.activeSelf)
        {
            //ButtonSettings.SetActive(false);
            StartCoroutine(MoveImage(center_coord_for_settings_button - ButtonSettings.GetComponent<RectTransform>().anchoredPosition,
                                     ButtonSettings.GetComponent<Image>(), 0.25f, true));
            StartCoroutine(RotateImage(-40f, ButtonSettings.GetComponent<Image>(), 0.25f));

            StartCoroutine(ChangeStateWithDelay(SettingsMenu, 0.25f, true));
        }
    }

    public void CloseSettingsMenu() 
    {
        if (SettingsMenu.activeSelf)
        {
            SettingsMenu.SetActive(false);
            ButtonSettings.SetActive(true);
            StartCoroutine(MoveImage(old_pos_of_settings_button - ButtonSettings.GetComponent<RectTransform>().anchoredPosition,
                                     ButtonSettings.GetComponent<Image>(), 0.25f, false));

            StartCoroutine(RotateImage(40f, ButtonSettings.GetComponent<Image>(), 0.25f));
            //StartCoroutine(ChangeStateWithDelay(ButtonSettings,0.25f,true));
        }
    }

    private IEnumerator ChangeStateWithDelay(GameObject obj, float delay, bool state) 
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(state);
    }


}
