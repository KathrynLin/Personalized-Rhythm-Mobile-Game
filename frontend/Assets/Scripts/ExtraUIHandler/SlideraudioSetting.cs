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
        // 初始化滑动条的值为 ScriptableObject 中的值
        volumeSlider.value = volumeSettings.audio;
        UpdateVolumeText(volumeSlider.value);

        // 添加滑动条值变化的监听器
        volumeSlider.onValueChanged.AddListener(OnVolumeSliderValueChanged);

        // 设置初始音量
        SetVolume(volumeSettings.audio);
    }

    void OnVolumeSliderValueChanged(float value)
    {
        // 更新 ScriptableObject 中的值
        volumeSettings.audio = value;
        // 更新音量显示文本
        UpdateVolumeText(value);
        // 更新 AudioSource 的音量
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
