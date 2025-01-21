using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

public class SignupController : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField usernameField;
    public TMP_InputField passwordField;
    public Button signupButton;
    public TMP_Text userIdText;
    public TMP_Text worldRankText;

    public GameObject signupPanel; // Reference to the SignupPanel GameObject in Unity
    public UserStats userStats; // Reference to the UserStats ScriptableObject

    private string baseUrl = "https://capstone.fangqinglin.com/api"; // Replace with your server URL

    public void Start()
    {
        signupButton.onClick.AddListener(OnSignupButtonClicked);
    }

    private IEnumerator SignupRequest(string username, string password)
    {

        // yield return SubmitScore(username, 0);

        string url = baseUrl + "/register";
        string json = "{\"username\": \"" + username + "\", \"password\": \"" + password + "\"}";

        // Convert JSON to byte array
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to register: " + www.error);
            }
            else
            {
                string responseJson = www.downloadHandler.text;
                SignupResponse response = JsonUtility.FromJson<SignupResponse>(responseJson);
                
                // Log server response message
                Debug.Log("Server response: " + response.message);

                // Check if registration was successful
                if (response.message == "User registered successfully")
                {
                    // Update userIdText
                    userIdText.text = username;

                    // Update worldrank and id from UserStats
                    userStats.id = username;
                    
                    // Submit initial score of 0 and retrieve world rank
                    StartCoroutine(SubmitScore(username, 0));

                    

                }
                else
                {
                    Debug.LogWarning("Registration failed: " + response.message);
                }
            }
        }
    }

    private IEnumerator SubmitScore(string username, int score)
    {
        string url = baseUrl + "/score";
        string json = "{\"username\": \"" + username + "\", \"score\": " + score + "}";

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to submit score: " + www.error);
            }
            else
            {
                Debug.Log("Score submitted successfully");
                // Get the world rank after submitting the score
                StartCoroutine(GetWorldRank(username));
            }
        }
    }

    private IEnumerator GetWorldRank(string username)
    {
        string url = baseUrl + "/rank?username=" + UnityWebRequest.EscapeURL(username);

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to get world rank: " + www.error);
            }
            else
            {
                string responseJson = www.downloadHandler.text;
                RankResponse response = JsonUtility.FromJson<RankResponse>(responseJson);
                
                if (response != null)
                {
                    userStats.worldrank = response.world_rank;
                    worldRankText.text = "World Rank: " + response.world_rank;
                    Debug.Log("World rank retrieved successfully: " + response.world_rank);

                    // Hide the signup panel
                    signupPanel.SetActive(false);
                }
                else
                {
                    Debug.LogWarning("Failed to retrieve world rank");
                }
            }
        }
    }

    private void OnSignupButtonClicked()
    {
        // Ensure the button is active before starting the coroutine
        if (signupButton.gameObject.activeInHierarchy)
        {
            string username = usernameField.text;
            string password = passwordField.text;

            StartCoroutine(SignupRequest(username, password));
        }
    }
}

[System.Serializable]
public class SignupResponse
{
    public string message;
}

[System.Serializable]
public class RankResponse
{
    public int world_rank;
}


