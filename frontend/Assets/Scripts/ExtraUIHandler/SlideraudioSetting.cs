using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI volumeText;

    [Header("Settings")]
    [SerializeField] private SettingStats volumeSettings;

    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    void Start()
    {
        // ��ʼ����������ֵΪ ScriptableObject �е�ֵ
        volumeSlider.value = volumeSettings.audio;
        UpdateVolumeText(volumeSlider.value);

        // ��ӻ�����ֵ�仯�ļ�����
        volumeSlider.onValueChanged.AddListener(OnVolumeSliderValueChanged);

        // ���ó�ʼ����
        SetVolume(volumeSettings.audio);
    }

    void OnVolumeSliderValueChanged(float value)
    {
        // ���� ScriptableObject �е�ֵ
        volumeSettings.audio = value;
        // ����������ʾ�ı�
        UpdateVolumeText(value);
        // ���� AudioSource ������
        SetVolume(value);
    }

    private void UpdateVolumeText(float value)
    {
        volumeText.text = (value * 100).ToString("0.0") + "%";
        volumeText.color = Color.black;
    }

    private void SetVolume(float value)
    {
        audioSource.volume = value;
    }
}
