using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class StaminaUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image radialImage;

    [Header("Thresholds (0..1)")]
    [Range(0f, 1f)] [SerializeField] private float yellowAt = 0.5f;
    [Range(0f, 1f)] [SerializeField] private float redAt = 0.2f;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.green;
    [SerializeField] private Color yellowColor = Color.yellow;
    [SerializeField] private Color redColor = Color.red;

    private PlayerStatManager stats;

    private void Awake()
    {
        if (radialImage == null)
            radialImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        stats = PlayerStatManager.Instance;
        if (stats != null)
        {
            stats.OnStaminaChanged += HandleStaminaChanged;
            HandleStaminaChanged(stats.Stamina);
        }
    }

    private void OnDisable()
    {
        if (stats != null)
            stats.OnStaminaChanged -= HandleStaminaChanged;
    }

    private void HandleStaminaChanged(float currentStamina)
    {
        if (stats == null || radialImage == null) return;

        float max = Mathf.Max(1f, stats.StaminaMax);
        float t = Mathf.Clamp01(currentStamina / max);

        radialImage.fillAmount = t;

        if (t <= redAt) radialImage.color = redColor;
        else if (t <= yellowAt) radialImage.color = yellowColor;
        else radialImage.color = normalColor;
    }
}
