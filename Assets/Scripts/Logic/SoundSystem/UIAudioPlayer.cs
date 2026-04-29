using UnityEngine;
using UnityEngine.UI;

// This class responsible for playing ui click sound when attached on ui button
// In furure should be removed and implemented directly in UI for better flexibility
public class UIAudioPlayer : MonoBehaviour
{
    [SerializeField] UISoundId soundId = UISoundId.UIClick;

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

