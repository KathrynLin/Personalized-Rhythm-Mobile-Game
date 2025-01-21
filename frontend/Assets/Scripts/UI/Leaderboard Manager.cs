using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SocialPlatforms;

public class LeaderBoardManager : MonoBehaviour
{
    public Button LeaderboardButton;
    public GameObject LeaderBoardPanel;
    [Header("Stats")]
    [SerializeField] private UserStats stats;


    [Header("Text")]
    [SerializeField] private TextMeshProUGUI fstid;
    [SerializeField] private TextMeshProUGUI sedid;
    [SerializeField] private TextMeshProUGUI trdid;
    [SerializeField] private TextMeshProUGUI fierdid;
    [SerializeField] private TextMeshProUGUI funfndid;
      
    [SerializeField] private TextMeshProUGUI fstscore;
    [SerializeField] private TextMeshProUGUI sedscore;
    [SerializeField] private TextMeshProUGUI trdscore;
    [SerializeField] private TextMeshProUGUI fierscore;
    [SerializeField] private TextMeshProUGUI funfscore;

    private string baseUrl = "https://capstone.fangqinglin.com/api"; // Replace with your server URL
    public void Start()
    {
        LeaderboardButton.onClick.AddListener(OnLeaderBoardButtonClick);
    }

    private void OnLeaderBoardButtonClick()
    {
        if (stats.loginflag && LeaderBoardPanel.activeSelf) StartCoroutine(Requestleaderboarddata());
    }

    private IEnumerator Requestleaderboarddata()
    {
        //¸ÄÒ»ÂÖ
        string url = baseUrl + "/leaderboard" ;

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to Leaderboard Information: " + www.error);
            }
            else
            {
                string responseJson = www.downloadHandler.text;
                //RankResponse response = JsonUtility.FromJson<RankResponse>(responseJson);
                RankResponse response = JsonUtility.FromJson<RankResponse>("{\"ranks\":" + responseJson + "}");
                if (response != null && response.ranks.Length >= 5)
                {
                    fstid.text = response.ranks[0].username;
                    sedid.text = response.ranks[1].username;
                    trdid.text = response.ranks[2].username;
                    fierdid.text = response.ranks[3].username;
                    funfndid.text = response.ranks[4].username;

                    fstscore.text = response.ranks[0].score.ToString();
                    sedscore.text = response.ranks[1].score.ToString();
                    trdscore.text = response.ranks[2].score.ToString();
                    fierscore.text = response.ranks[3].score.ToString();
                    funfscore.text = response.ranks[4].score.ToString();
                }
                else
                {
                    Debug.LogWarning("Failed to retrieve leaderboard information");
                }
            }
        }
    }
    [System.Serializable]
    public class RankEntry
    {
        public int score;
        public string username;
        public int world_rank;
    }

    [System.Serializable]
    public class RankResponse
    {
        public RankEntry[] ranks;
    }

} 