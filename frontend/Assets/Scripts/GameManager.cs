using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine.Networking;
using UnityEngine.SocialPlatforms;

using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField] UserStats userstats;
    private string baseUrl = "https://capstone.fangqinglin.com/api"; // Replace with your server URL
    static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            return instance;
        }
    }

    public enum GameState
    {
        Game,
        Edit,
    }
    public GameState state = GameState.Game;

    /// <summary>
    /// game progress. InputManager.OnEnter() reference
    /// </summary>
    public bool isPlaying = true;
    public string title;
    Coroutine coPlaying;

    Judgement judgement = null;

    public Dictionary<string, Sheet> sheets = new Dictionary<string, Sheet>();

    [Header("Stats")]
    [SerializeField] private SettingStats stats;

    float speed;

    public float Speed
    {
        get
        {
            return speed;
        }
        set
        {
            speed = Mathf.Clamp(value, 1.0f, 5.0f);
        }
    }

    public List<GameObject> canvases = new List<GameObject>();
    enum Canvas
    {
        Title,
        Select,
        SFX,
        GameBGA,
        Game,
        Result,
        Editor,
    }
    CanvasGroup sfxFade;

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    void Start()
    {
        Application.targetFrameRate = 1200;
        speed = stats.speed;

        InputSystem.EnableDevice(UnityEngine.InputSystem.Gyroscope.current);

        StartCoroutine(IEInit());
    }

    private void Update()
    {
        if (judgement && judgement.initialized)
        {
            float leftRotSpeed = 3.0f;
            float rightRotSpeed = -3.0f;

            Vector3 vector = UnityEngine.InputSystem.Gyroscope.current.angularVelocity.ReadValue();
            if (vector.z > leftRotSpeed)
            {
                judgement.Judge(4);
            }
            else if (vector.z < rightRotSpeed)
            {
                judgement.Judge(5);
            }
        }
    }

    public void ChangeMode(UIObject uiObject)
    {
        if (state == GameState.Game)
        {
            state = GameState.Edit;
            TextMeshProUGUI text = uiObject.transform.GetComponentInChildren<TextMeshProUGUI>();
            text.text = "Edit\nMode";
        }
        else
        {
            state = GameState.Game;
            TextMeshProUGUI text = uiObject.transform.GetComponentInChildren<TextMeshProUGUI>();
            text.text = "Game\nMode";
        }
    }

    public void Title()
    {
        StartCoroutine(IETitle());
    }

    public void Select()
    {
        StartCoroutine(IESelect());
    }

    public void Play()
    {
        judgement = FindObjectOfType<Judgement>();
        StartCoroutine(IEInitPlay());
    }

    public void Edit()
    {
        StartCoroutine(IEEdit());
    }

    public void Stop()
    {
        if (state == GameState.Game)
        {
            // Game UI Off
            canvases[(int)Canvas.Game].SetActive(false);

            // playing timer Off
            if (coPlaying != null)
            {
                StopCoroutine(coPlaying);
                coPlaying = null;
            }
        }
        else
        {
            // Editor UI Off
            canvases[(int)Canvas.Editor].SetActive(false);
            Editor.Instance.Stop();

            FindObjectOfType<GridGenerator>().InActivate();

            // There may be objects that have been modified in the editor, so they are updated.
            // TODO: should differ between external sheets and internal sheets
            // such that internal sheets should be restored to external storage
            //StartCoroutine(Parser.Instance.IEParseInternal(title));
            sheets[title] = Parser.Instance.sheet;
        }

        // Turn off Note Gen
        NoteGenerator.Instance.StopGen();

        // turn off music
        AudioManager.Instance.progressTime = 0f;
        AudioManager.Instance.Stop();

        judgement.initialized = false;
        judgement = null;
        Select();
    }

    [System.Serializable]
    public class ScoreData
    {
        public int score;
    }

    private IEnumerator SubmitScore(int score)
    {
        string url = baseUrl + "/score";
        string token = PlayerPrefs.GetString("JWT", "blank");
        // 创建 ScoreData 对象并设置分数
        ScoreData scoreData = new ScoreData();
        scoreData.score = score;

        // 将 ScoreData 对象序列化为 JSON 字符串
        string json = JsonUtility.ToJson(scoreData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();

            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", "Bearer " + token);

            yield return www.SendWebRequest();
            /*if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(bodyRaw);
                Debug.LogError("Token" + token);
                Debug.LogError("Failed to Post Score: " + www.error);
            }
            else
            {
                Debug.LogError("score upload success");
            }*/
           
        }
    }
    IEnumerator IEInit()
    {
        SheetLoader.Instance.Init();

        foreach (GameObject go in canvases)
        {
            go.SetActive(true);
        }
        sfxFade = canvases[(int)Canvas.SFX].GetComponent<CanvasGroup>();
        sfxFade.alpha = 1f;

        UIController.Instance.Init();
        Score.Instance.Init();

        // Give UIObjects some time to cache themselves and disable them (temporary code)
        yield return new WaitForSeconds(2f);
        canvases[(int)Canvas.Game].SetActive(false);
        canvases[(int)Canvas.GameBGA].SetActive(false);
        canvases[(int)Canvas.Result].SetActive(false);
        canvases[(int)Canvas.Select].SetActive(false);
        canvases[(int)Canvas.Editor].SetActive(false);

        // Create selection screen item
        yield return new WaitUntil(() => SheetLoader.Instance.bLoadFinish == true);
        ItemGenerator.Instance.Init();

        // Start title screen
        Title();
    }

    IEnumerator IETitle()
    {
        // screen fade in
        canvases[(int)Canvas.SFX].SetActive(true);
        yield return StartCoroutine(AniPreset.Instance.IEAniFade(sfxFade, false, 1f));

        // Play title intro
        canvases[(int)Canvas.Title].GetComponent<Animation>().Play();
        yield return new WaitForSeconds(5.6f);

        // Start selection screen
        Select();
    }

    /**
     * TODO: reload external sheets on re-entrance
     */
    IEnumerator IESelect()
    {
        // screen fade out
        canvases[(int)Canvas.SFX].SetActive(true);
        yield return StartCoroutine(AniPreset.Instance.IEAniFade(sfxFade, true, 2f));

        // Title UI Off
        canvases[(int)Canvas.Title].SetActive(false);

        // Result UI Off
        canvases[(int)Canvas.Result].SetActive(false);

        // Select UI On
        canvases[(int)Canvas.Select].SetActive(true);

        // screen fade in
        yield return StartCoroutine(AniPreset.Instance.IEAniFade(sfxFade, false, 2f));
        canvases[(int)Canvas.SFX].SetActive(false);

        // Allows you to start a new game
        isPlaying = false;
    }

    IEnumerator IEInitPlay()
    {
        // Prevents you from starting new games
        isPlaying = true;

        // screen fade out
        canvases[(int)Canvas.SFX].SetActive(true);
        yield return StartCoroutine(AniPreset.Instance.IEAniFade(sfxFade, true, 2f));

        //  Select UI Off
        canvases[(int)Canvas.Select].SetActive(false);


        // Sheet reset
        title = sheets.ElementAt(ItemController.Instance.page).Key;
        sheets[title].Init();

        // Audio insertion
        AudioManager.Instance.Insert(sheets[title].clip);

        // Game UI On
        canvases[(int)Canvas.Game].SetActive(true);

        // BGA On
        canvases[(int)Canvas.GameBGA].SetActive(true);

        // Judgment reset
        FindObjectOfType<Judgement>().Init();

        // Reset score
        Score.Instance.Clear();

        // Judgment effect reset
        JudgeEffect.Instance.Init();

        // screen fade in
        yield return StartCoroutine(AniPreset.Instance.IEAniFade(sfxFade, false, 2f));
        canvases[(int)Canvas.SFX].SetActive(false);

        // Note produce
        NoteGenerator.Instance.StartGen();

        // wait 3 seconds
        yield return new WaitForSeconds(3f);

        // Audio play
        AudioManager.Instance.progressTime = 0f;
        AudioManager.Instance.Play();

        // End reminder
        coPlaying = StartCoroutine(IEEndPlay());
    }

    // game over
    IEnumerator IEEndPlay()
    {
        while (true)
        {
            if (!AudioManager.Instance.IsPlaying())
            {
                break;
            }
            yield return new WaitForSeconds(1f);
        }

        // screen fade out
        canvases[(int)Canvas.SFX].SetActive(true);
        yield return StartCoroutine(AniPreset.Instance.IEAniFade(sfxFade, true, 2f));
        canvases[(int)Canvas.Game].SetActive(false);
        canvases[(int)Canvas.GameBGA].SetActive(false);
        canvases[(int)Canvas.Result].SetActive(true);

        UIText rscore = UIController.Instance.FindUI("UI_R_Score").uiObject as UIText;
        UIText rgreat = UIController.Instance.FindUI("UI_R_Great").uiObject as UIText;
        UIText rgood = UIController.Instance.FindUI("UI_R_Good").uiObject as UIText;
        UIText rmiss = UIController.Instance.FindUI("UI_R_Miss").uiObject as UIText;

        rscore.SetText(Score.Instance.data.score.ToString());
        rgreat.SetText(Score.Instance.data.great.ToString());
        rgood.SetText(Score.Instance.data.good.ToString());
        rmiss.SetText(Score.Instance.data.miss.ToString());

        UIImage rBG = UIController.Instance.FindUI("UI_R_BG").uiObject as UIImage;
        rBG.SetSprite(sheets[title].img);

        NoteGenerator.Instance.StopGen();
        AudioManager.Instance.Stop();

        // screen fade in
        yield return StartCoroutine(AniPreset.Instance.IEAniFade(sfxFade, false, 2f));
        canvases[(int)Canvas.SFX].SetActive(false);

        StartCoroutine(SubmitScore(Score.Instance.data.score));

        // wait 5 seconds
        yield return new WaitForSeconds(3f);

        // Call up the selection screen
        Select();
    }

    IEnumerator IEEdit()
    {
        // Prevents you from starting new games
        isPlaying = true;

        // screen fade out
        canvases[(int)Canvas.SFX].SetActive(true);
        yield return StartCoroutine(AniPreset.Instance.IEAniFade(sfxFade, true, 2f));

        //  Select UI Off
        canvases[(int)Canvas.Select].SetActive(false);

        // Sheet reset
        title = sheets.ElementAt(ItemController.Instance.page).Key;
        sheets[title].Init();

        // Audio insertion
        AudioManager.Instance.Insert(sheets[title].clip);

        // Grid produce
        FindObjectOfType<GridGenerator>().Init();

        // Note produce
        NoteGenerator.Instance.GenAll();

        // Editor UI On
        canvases[(int)Canvas.Editor].SetActive(true);

        // Editor reset
        Editor.Instance.Init();


        // screen fade in
        yield return StartCoroutine(AniPreset.Instance.IEAniFade(sfxFade, false, 2f));
        canvases[(int)Canvas.SFX].SetActive(false);
    }
}
