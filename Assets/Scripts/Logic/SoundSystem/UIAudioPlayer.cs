using UnityEngine;
using UnityEngine.UI;

public class UIAudioPlayer : MonoBehaviour
{
    [SerializeField] private UISoundId soundId = UISoundId.UIClick;

    public void Start()
    {
        if (TryGetComponent<Button>(out var button))
        {
            button.onClick.AddListener(PlayDefault);
        }
    }
    public void PlayDefault()
    {
        SoundManager.Instance?.PlayUI(soundId);
    }

}

