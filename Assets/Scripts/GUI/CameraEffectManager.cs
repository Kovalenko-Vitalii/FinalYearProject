using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static GameplayOrchestrator;

public class CameraEffectManager : MonoBehaviour
{
    [Header("Volume")]
    [SerializeField] private Volume volume;

    private Vignette vignette;
    private DepthOfField depthOfField;
    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;

    [Header("Sensitivity")]
    public float maxVignetteFromSnapshot;
    public float maxBlurStrength;
    public float maxChromaticFromSnapshot;
    public float pulseSpeed;

    private void Awake()
    {
        if (volume == null)
        {
            volume = GetComponent<Volume>();
        }

        if (volume != null && volume.profile != null)
        {
            volume.profile.TryGet(out vignette);
            volume.profile.TryGet(out depthOfField);
            volume.profile.TryGet(out chromaticAberration);
            volume.profile.TryGet(out lensDistortion);
        }
    }

    private void Update()
    {
        if (GameplayOrchestrator.Instance.State != GameState.Gameplay) return;
        if (PauseManager.Instance.IsPaused) return;

        var mgr = StatusEffectManager.Instance;
        if (mgr == null || volume == null || volume.profile == null)
            return;

        var snapshot = mgr.CurrentSnapshot;

        float effectivePain = snapshot.PainIntensity * (1f - snapshot.PainSuppression);
        effectivePain = Mathf.Clamp01(effectivePain);

        if (vignette != null)
        {
            float baseVignette = effectivePain * maxVignetteFromSnapshot;

            float pulseFactor = 1f;
            if (effectivePain > 0f)
            {
                float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
                pulseFactor = 1f + pulse * 0.3f * effectivePain;
            }

            vignette.intensity.value = baseVignette * pulseFactor;
        }

        if (depthOfField != null)
        {
            float blur = effectivePain * maxBlurStrength;

            depthOfField.active = blur > 0.01f;
            depthOfField.gaussianStart.value = Mathf.Lerp(10f, 2f, blur);
            depthOfField.gaussianEnd.value = Mathf.Lerp(40f, 5f, blur);
        }

        if (chromaticAberration != null)
        {
            float dv = effectivePain * maxChromaticFromSnapshot;

            if (effectivePain > 0f)
            {
                float pulse = (Mathf.Sin(Time.time * pulseSpeed * 0.9f) + 1f) * 0.5f;
                dv *= Mathf.Lerp(0.8f, 1.2f, pulse);
            }

            chromaticAberration.active = dv > 0.01f;
            chromaticAberration.intensity.value = dv;
        }

        if (lensDistortion != null)
        {
            float strongPain = Mathf.Clamp01(effectivePain * 1.2f);

            float strength = 0f;
            if (strongPain > 0.01f)
            {
                float pulse = (Mathf.Sin(Time.time * (pulseSpeed * 0.7f)) + 1f) * 0.5f;
                pulse *= pulse;

                strength = strongPain * 0.2f * pulse;

                lensDistortion.active = true;
                lensDistortion.intensity.value = strength;
            }
            else
            {
                lensDistortion.intensity.value = 0f;
                lensDistortion.active = false;
            }
        }

    }

}
