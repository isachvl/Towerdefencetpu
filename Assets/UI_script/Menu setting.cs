using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("Graphics Toggles")]
    [SerializeField] private Toggle lowToggle;
    [SerializeField] private Toggle mediumToggle;
    [SerializeField] private Toggle highToggle;

    [Header("Audio")]
    [SerializeField] private Slider masterVolumeSlider;

    [Header("Buttons")]
    [SerializeField] private Button closeButton;

    [Header("Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject startMenuPanel;

    private const string QualityKey = "QualityLevel";
    private const string VolumeKey = "MasterVolume";

    private void Start()
    {
        LoadSettings();

        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);

        if (lowToggle != null)
        {
            lowToggle.onValueChanged.AddListener(delegate
            {
                if (lowToggle.isOn)
                    SetQuality(0);
            });
        }

        if (mediumToggle != null)
        {
            mediumToggle.onValueChanged.AddListener(delegate
            {
                if (mediumToggle.isOn)
                    SetQuality(1);
            });
        }

        if (highToggle != null)
        {
            highToggle.onValueChanged.AddListener(delegate
            {
                if (highToggle.isOn)
                    SetQuality(2);
            });
        }

        if (closeButton != null)
            closeButton.onClick.AddListener(BackToStartMenu);
    }

    private void LoadSettings()
    {
        int savedQuality = PlayerPrefs.GetInt(QualityKey, 1);
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);

        ApplyQualityWithoutNotify(savedQuality);

        AudioListener.volume = savedVolume;

        if (masterVolumeSlider != null)
            masterVolumeSlider.SetValueWithoutNotify(savedVolume);
    }

    private void ApplyQualityWithoutNotify(int qualityIndex)
    {
        qualityIndex = Mathf.Clamp(qualityIndex, 0, 2);
        QualitySettings.SetQualityLevel(qualityIndex, true);

        if (lowToggle != null)
            lowToggle.SetIsOnWithoutNotify(qualityIndex == 0);

        if (mediumToggle != null)
            mediumToggle.SetIsOnWithoutNotify(qualityIndex == 1);

        if (highToggle != null)
            highToggle.SetIsOnWithoutNotify(qualityIndex == 2);
    }

    public void SetQuality(int qualityIndex)
    {
        qualityIndex = Mathf.Clamp(qualityIndex, 0, 2);
        QualitySettings.SetQualityLevel(qualityIndex, true);
        PlayerPrefs.SetInt(QualityKey, qualityIndex);
        PlayerPrefs.Save();
    }

    public void SetMasterVolume(float value)
    {
        value = Mathf.Clamp01(value);
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(VolumeKey, value);
        PlayerPrefs.Save();
    }

    public void OpenSettings()
    {
        if (startMenuPanel != null)
            startMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        LoadSettings();
    }

    public void BackToStartMenu()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (startMenuPanel != null)
            startMenuPanel.SetActive(true);
    }
}