using UnityEngine;
using UnityEngine.UI;

public class LeaderBoardHandler : MonoBehaviour
{
    public GameObject LeaderBoardPanel;
    public GameObject ErrorPannel;
    public Button LeaderBoardButton;
    public Button exitbutton;
    [SerializeField] private UserStats stats;

    void Start()
    {
        LeaderBoardPanel.SetActive(false);
        ErrorPannel.SetActive(false);

        // 绑定 exitbutton 的点击事件
        exitbutton.onClick.AddListener(CloseErrorPanel);
    }

    public void ShowLeaderBoardPanel()
    {
        if (!stats.loginflag)
        {
            ErrorPannel.SetActive(true);
            return;
        }
        // 打开排行榜
        LeaderBoardPanel.SetActive(!LeaderBoardPanel.activeSelf);
    }

    public void CloseErrorPanel()
    {
        ErrorPannel.SetActive(false);
    }
}
