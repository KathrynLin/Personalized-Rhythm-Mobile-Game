using UnityEngine;
using UnityEngine.UI;

public class PanelToggleHandler : MonoBehaviour
{
    public GameObject SettingPanel;

    void Start()
    {
        // 确保面板初始不可见
        SettingPanel.SetActive(false);
    }

    public void TogglePanelVisibility()
    {
        // 切换面板的可见性
        SettingPanel.SetActive(!SettingPanel.activeSelf);
    }
}