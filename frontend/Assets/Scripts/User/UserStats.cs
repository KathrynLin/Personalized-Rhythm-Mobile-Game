using UnityEngine;

[CreateAssetMenu(fileName = "UserStats", menuName = "User Stats")]
public class UserStats : ScriptableObject
{
    public int worldrank;
    public Sprite icon; // Close
    public string id;
    public bool loginflag;

    private void OnEnable()
    {
        worldrank = 9999;
        id = "testuser";
        loginflag = false;
    }
}