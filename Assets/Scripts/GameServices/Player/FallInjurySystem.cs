using UnityEngine;

// This script responsible for adding injuries when player fell off some hIgh place
[RequireComponent(typeof(CharacterController))]
public class FallInjurySystem : MonoBehaviour, IPlayerTick
{
    [Header("Links")]
    [SerializeField] PlayerMovement movement;

    [Header("Fall thresholds")]
    [SerializeField] float safeFallHeight = 2.5f;
    [SerializeField] float fractureFallHeight = 4.5f;
    [SerializeField] float hardFallHeight = 7.0f;

    [Header("Impact speed (optional)")]
    [SerializeField] float fractureImpactSpeed = 12f;
    [SerializeField] float hardImpactSpeed = 16f;

    [Header("Effect tuning")]
    [SerializeField] float injuryDuration = 180f;
    [SerializeField] float fractureSpeedMultiplier = 0.55f;

    [SerializeField] float painIntensityMin = 0.25f;
    [SerializeField] float painIntensityMax = 0.85f;

    [SerializeField] AudioClip brokenBoneSound;

    CharacterController cc;

    bool wasGrounded;
    float fallStartY;
    float minVerticalVelocity;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (movement == null) movement = GetComponent<PlayerMovement>();
    }

    void OnEnable() => PlayerTickSystem.Instance?.Register(this);
    void OnDisable() => PlayerTickSystem.Instance?.Unregister(this);

    public void Tick(float dt)
    {
        if (movement == null || StatusEffectManager.Instance == null) return;

        bool grounded = cc.isGrounded;

        // Checking if player was grounded previously and comparing to current state
        // zeroing starting fall point "fallStartY" and recording our fall start poition
        // it is necessary to determine height
        if (wasGrounded && !grounded)
        {
            fallStartY = transform.position.y;
            minVerticalVelocity = 0f;
        }

        // Getting highest velocity
        if (!grounded)
        {
            float vy = movement.VerticalVelocity;
            if (vy < minVerticalVelocity) minVerticalVelocity = vy;
        }

        if (!wasGrounded && grounded)
        {
            float fallHeight = fallStartY - transform.position.y;
            float impactSpeed = -minVerticalVelocity;

            EvaluateFall(fallHeight, impactSpeed);
        }

        wasGrounded = grounded;
    }

    private void EvaluateFall(float fallHeight, float impactSpeed)
    {
        if (fallHeight < safeFallHeight && impactSpeed < fractureImpactSpeed)
            return;

        float tHeight = Mathf.InverseLerp(fractureFallHeight, hardFallHeight, fallHeight);
        float tSpeed = Mathf.InverseLerp(fractureImpactSpeed, hardImpactSpeed, impactSpeed);

        float severity = Mathf.Clamp01(Mathf.Max(tHeight, tSpeed));

        

        float fractureChance = Mathf.Lerp(0.25f, 1.0f, severity);
        if (Random.value < fractureChance)
        {   
            float painIntensity = Mathf.Lerp(painIntensityMin, painIntensityMax, severity);
            StatusEffectManager.Instance.AddEffect(new PainEffect(
                duration: injuryDuration,
                intensity: painIntensity,
                target: BodyPart.Head
            ));
            BodyPart leg = (Random.value < 0.5f) ? BodyPart.LeftLeg : BodyPart.RightLeg;

            StatusEffectManager.Instance.AddEffect(new FractureEffect(
                duration: injuryDuration * 2,
                speedMultiplier: fractureSpeedMultiplier,
                targetPart: leg
            ));

            SoundManager.Instance.PlayUI(brokenBoneSound);
            PlayerStatManager.Instance.ChangeHealth(severity * -100);

            if (severity > 0.9f && Random.value < 0.35f)
            {
                BodyPart otherLeg = (leg == BodyPart.LeftLeg) ? BodyPart.RightLeg : BodyPart.LeftLeg;
                StatusEffectManager.Instance.AddEffect(new FractureEffect(
                    duration: injuryDuration * 2,
                    speedMultiplier: fractureSpeedMultiplier,
                    targetPart: otherLeg
                ));
            }
        }
    }
}