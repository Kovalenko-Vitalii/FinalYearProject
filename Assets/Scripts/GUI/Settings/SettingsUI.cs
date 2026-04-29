using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider uiSlider;
    [SerializeField] private Slider subtitleSlider;
    [SerializeField] private Slider worldSlider;

    private void Start()
    {
        if (SoundManager.Instance == null) return;

        masterSlider.SetValueWithoutNotify(SoundManager.Instance.MasterVolume);
        uiSlider.SetValueWithoutNotify(SoundManager.Instance.UIVolume);
        subtitleSlider.SetValueWithoutNotify(SoundManager.Instance.SubtitleVolume);
        worldSlider.SetValueWithoutNotify(SoundManager.Instance.WorldVolume);

        masterSlider.onValueChanged.AddListener(OnMasterChanged);
        uiSlider.onValueChanged.AddListener(OnUIChanged);
        subtitleSlider.onValueChanged.AddListener(OnSubtitleChanged);
        worldSlider.onValueChanged.AddListener(OnWorldChanged);
    }

    private void OnDestroy()
    {
        masterSlider.onValueChanged.RemoveListener(OnMasterChanged);
        uiSlider.onValueChanged.RemoveListener(OnUIChanged);
        subtitleSlider.onValueChanged.RemoveListener(OnSubtitleChanged);
        worldSlider.onValueChanged.RemoveListener(OnWorldChanged);
    }

    private void OnMasterChanged(float value)
    {
        SoundManager.Instance.SetMasterVolume(value);
    }

    private void OnUIChanged(float value)
    {
        SoundManager.Instance.SetUIVolume(value);
    }

    private void OnSubtitleChanged(float value)
    {
        SoundManager.Instance.SetSubtitleVolume(value);
    }

    private void OnWorldChanged(float value)
    {
        SoundManager.Instance.SetWorldVolume(value);
    }
}