using UnityEngine;

public class LoginHandler : MonoBehaviour
{
    public GameObject loginPanel;
    public GameObject signupPanel;

    void Start()
    {
        // 初始时确保登录面板和注册面板都关闭
        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
    }

    public void TogglePanelVisibility()
    {
        bool anyPanelActive = loginPanel.activeSelf || signupPanel.activeSelf;

        if (anyPanelActive)
        {
            loginPanel.SetActive(false);
            signupPanel.SetActive(false);
        }
        else
        {
            loginPanel.SetActive(true);
        }
    }

}
