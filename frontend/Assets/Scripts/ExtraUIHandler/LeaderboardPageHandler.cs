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

        // �� exitbutton �ĵ���¼�
        exitbutton.onClick.AddListener(CloseErrorPanel);
    }

    public void ShowLeaderBoardPanel()
    {
        if (!stats.loginflag)
        {
            ErrorPannel.SetActive(true);
            return;
        }
        // �����а�
        LeaderBoardPanel.SetActive(!LeaderBoardPanel.activeSelf);
    }

    public void CloseErrorPanel()
    {
        ErrorPannel.SetActive(false);
    }
}
