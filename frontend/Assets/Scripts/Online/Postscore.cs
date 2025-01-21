using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class Postscore : MonoBehaviour
{
    private string baseUrl = "https://capstone.fangqinglin.com/api"; // Replace with your server URL
    public Button postdebugeer;
    [SerializeField] private UserStats userstats;

    public void Start()
    {
        postdebugeer.onClick.AddListener(startpost);
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

            www.SetRequestHeader("Content-Type", "application/json" );
            www.SetRequestHeader("Authorization", "Bearer " + token);

            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(bodyRaw);
                Debug.LogError("Token" + token);
                Debug.LogError("Failed to Post Score: " + www.error);
            }
            else
            {
                string responseJson = www.downloadHandler.text;
                LoginResponse response = JsonUtility.FromJson<LoginResponse>(responseJson);

                // Log server response message
                if (response != null) Debug.Log("Server response: " + response.message);

                // Check if login was successful

                else
                {
                    Debug.LogWarning("Login failed: " + response.message);
                }
            }
        }
    }

    private void startpost()
    {
        StartCoroutine(SubmitScore(6666));
    }
     
   
}
