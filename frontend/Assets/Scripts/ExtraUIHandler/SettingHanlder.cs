using UnityEngine;
using UnityEngine.UI;

public class PanelToggleHandler : MonoBehaviour
{
    public GameObject SettingPanel;

    void Start()
    {
        // ȷ������ʼ���ɼ�
        SettingPanel.SetActive(false);
    }

    public void TogglePanelVisibility()
    {
        // �л����Ŀɼ���
        SettingPanel.SetActive(!SettingPanel.activeSelf);
    }
}