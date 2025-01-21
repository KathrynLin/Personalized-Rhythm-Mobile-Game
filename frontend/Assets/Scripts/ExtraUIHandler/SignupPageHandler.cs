using UnityEngine;
using UnityEngine.UI;

public class SignupHandler : MonoBehaviour
{
    public GameObject loginPanel;
    public GameObject signupPanel;
    public Button signupButton;

    void Start()
    {
        // 确保启动时显示登录面板，隐藏注册面板
        loginPanel.SetActive(true);
        signupPanel.SetActive(false);

        // 绑定按钮点击事件
        signupButton.onClick.AddListener(ShowSignupPanel);
    }

    public void ShowSignupPanel()
    {
        // 切换注册面板的可见性，并隐藏登录面板
        signupPanel.SetActive(true);
        loginPanel.SetActive(false);
    }
}
