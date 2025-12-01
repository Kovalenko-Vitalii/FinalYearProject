using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraEffectManager : MonoBehaviour
{
    [Header("Volume")]
    [SerializeField] private Volume volume;

    private Vignette vignette;
    private DepthOfField depthOfField;
    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;

    [Header("Настройки чувствительности")]
    public float maxVignetteFromSnapshot = 0.45f;
    public float maxBlurStrength = 1f;
    public float maxChromaticFromSnapshot = 0.7f;
    public float pulseSpeed = 5f;

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
        var mgr = StatusEffectManager.Instance;
        if (mgr == null || volume == null || volume.profile == null)
            return;

        var snapshot = mgr.CurrentSnapshot;

        float pain = snapshot.PainSuppressed ? 0f : snapshot.PainIntensity;

        if (vignette != null)
        {
            float baseVignette = snapshot.VignetteIntensity * maxVignetteFromSnapshot;

            float pulseFactor = 1f;
            if (snapshot.PulseIntensity > 0f)
            {
                float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
                pulseFactor = 1f + pulse * 0.3f * snapshot.PulseIntensity;
            }

            vignette.intensity.value = baseVignette * pulseFactor;
        }

        if (depthOfField != null)
        {
            float blur = snapshot.ScreenBlur * maxBlurStrength;

            depthOfField.active = blur > 0.01f;
            depthOfField.gaussianStart.value = Mathf.Lerp(10f, 2f, blur);
            depthOfField.gaussianEnd.value = Mathf.Lerp(40f, 5f, blur);
        }

        if (chromaticAberration != null)
        {
            float dv = snapshot.DoubleVision * maxChromaticFromSnapshot;

            if (snapshot.PulseIntensity > 0f)
            {
                float pulse = (Mathf.Sin(Time.time * pulseSpeed * 0.9f) + 1f) * 0.5f;
                dv *= Mathf.Lerp(0.8f, 1.2f, pulse);
            }

            chromaticAberration.active = dv > 0.01f;
            chromaticAberration.intensity.value = dv;
        }

        if (lensDistortion != null)
        {
            float strongPain = Mathf.Clamp01(pain * 1.2f);
            if (strongPain > 0.01f)
            {
                float pulse = Mathf.Sin(Time.time * (pulseSpeed * 0.7f));
                float strength = strongPain * 0.2f * pulse;

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
