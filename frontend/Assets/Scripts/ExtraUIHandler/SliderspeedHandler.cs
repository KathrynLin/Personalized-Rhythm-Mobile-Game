using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI valueText;
    [Header("Settings")]
    [SerializeField] private SettingStats stats;
   
    void Start()
    {
        // ��ʼ����������ֵΪ ScriptableObject �е�ֵ
        slider.value = stats.speed;
        valueText.text = slider.value.ToString("0.0");

        // ���ӻ�����ֵ�仯�ļ�����
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    void OnSliderValueChanged(float value)
    {
        // ���� ScriptableObject �е�ֵ
        stats.speed = value;
        // ������ʾ���ı�
        valueText.text = value.ToString("0.0");
        valueText.color = Color.black;
        GameManager.Instance.Speed = value;
    }
}
