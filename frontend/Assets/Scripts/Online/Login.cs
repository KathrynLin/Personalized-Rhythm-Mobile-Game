//v0 success login
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

public class LoginController : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField usernameField;
    public TMP_InputField passwordField;
    public Button loginButton;
    public TMP_Text userIdText;
    public TMP_Text worldRankText;
    public GameObject loginPanel; // Reference to the LoginPanel GameObject in Unity
    public UserStats userStats; 

    private string baseUrl = "https://capstone.fangqinglin.com/api"; // Replace with your server URL

    public void Start()
    {
        loginButton.onClick.AddListener(OnLoginButtonClick);
    }

    private IEnumerator LoginRequest(string username, string password)
    {
        string url = baseUrl + "/login";
        string json = "{\"username\": \"" + username + "\", \"password\": \"" + password + "\"}";

        // Convert JSON to byte array
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
           
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to login: " + www.error);
            }
            else
            {
                userStats.loginflag = true;
                string responseJson = www.downloadHandler.text;
                LoginResponse response = JsonUtility.FromJson<LoginResponse>(responseJson);
                
                // Log server response message
                Debug.Log("Server response: " + response.message);

                // Check if login was successful
                if (response.message == "Login successful")
                {
                    // Example: Update UI elements with user information
                    userIdText.text = "User ID: " + response.userId;
                    // worldRankText.text = "World Rank: " + response.worldRank;
                    userStats.id = username;
                    Debug.Log(response.access_token);
                    SaveToken(response.access_token);
                    StartCoroutine(GetWorldRank(username));

                    // // Hide the login panel
                    // loginPanel.SetActive(false);
                }
                else
                {
                    Debug.LogWarning("Login failed: " + response.message);
                }
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
                    loginPanel.SetActive(false);
                }
                else
                {
                    Debug.LogWarning("Failed to retrieve world rank");
                }
            }
        }
    }

    public void SaveToken(string token)
    {
        PlayerPrefs.SetString("JWT", token);
        PlayerPrefs.Save();//Make sure to save PlayerPrefs changes
    }

    private void OnLoginButtonClick()
    {
        string username = usernameField.text;
        string password = passwordField.text;
        
        StartCoroutine(LoginRequest(username, password));
    }
}

[System.Serializable]
public class LoginResponse
{
    public string message;
    public string access_token;
    public string userId;
    public string worldRank;
}
