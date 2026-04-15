using System;
using UnityEngine;

public class HeldMeleeItem : PlayerHeldItem
{
    string TAG = "MELEE";

    [Header("Melee Animation")]
    [SerializeField] private Animator animator;

    [Header("VFX")]
    [SerializeField] private ImpactEffectDatabase impactDatabase;

    private bool isBusy;
    private bool isSprinting;
    private bool isHoldingAttack;
    private bool hitAppliedThisSwing;

    private int swingTriggerHash;

    private HoldableMeleeData meleeData;

    // === Initialization ===
    public override void Initialize(PlayerItemController owner, InventoryItem itemInstance)
    {
        base.Initialize(owner, itemInstance);

        meleeData = Data as HoldableMeleeData;
        if (meleeData == null)
        {
            GameLog.Error(TAG, $"[{nameof(HeldMeleeItem)}] Expected {nameof(HoldableMeleeData)}, got {Data?.GetType().Name}");
            return;
        }

        swingTriggerHash = Animator.StringToHash(meleeData.swingTriggerName);
    }
    protected override void Awake()
    {
        base.Awake();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }
    public override void OnEquip()
    {
        base.OnEquip();

        isBusy = false;
        isSprinting = false;
        isHoldingAttack = false;
        hitAppliedThisSwing = false;
    }
    public override void OnUnequip()
    {
        isBusy = false;
        isHoldingAttack = false;
        hitAppliedThisSwing = false;
    }

    // === Handling Input ===
    public override void OnPrimaryPressed()
    {
        if (isSprinting || PauseManager.Instance.IsPaused)
            return;

        isHoldingAttack = true;
        TryStartSwing();
    }
    public override void OnPrimaryReleased()
    {
        isHoldingAttack = false;
    }
    public override void OnSprintStarted()
    {
        isSprinting = true;
        isHoldingAttack = false;
        SetSprintPose();
    }
    public override void OnSprintStopped()
    {
        isSprinting = false;
        SetIdlePose();
    }

    // === Swing Logic ===
    private void TryStartSwing()
    {
        if (meleeData == null)
            return;

        if (isBusy || isSprinting)
            return;

        isBusy = true;
        hitAppliedThisSwing = false;

        if (animator != null)
            animator.SetTrigger(swingTriggerHash);

        if (audioSource != null && meleeData.swingSound != null)
            audioSource.PlayOneShot(meleeData.swingSound);
    }
    public void OnSwingHitFrame()
    {
        if (hitAppliedThisSwing)
            return;

        hitAppliedThisSwing = true;
        PerformHit();
    }
    public void OnSwingAnimationFinished()
    {
        isBusy = false;

        if (isHoldingAttack && !isSprinting)
            TryStartSwing();
    }
    private void PerformHit()
    {
        if (Owner == null || !Owner.TryGetAimRay(out Ray ray))
        {
            GameLog.Warning(TAG, "Aim ray is unavailable");
            return;
        }

        Debug.DrawRay(ray.origin, ray.direction * meleeData.hitDistance, Color.red, 1.5f);

        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            meleeData.hitDistance,
            meleeData.hitMask,
            QueryTriggerInteraction.Collide
        );

        if (hits == null || hits.Length == 0)
        {
            GameLog.Log(TAG, "No hits");
            return;
        }

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
             GameLog.Log(TAG, $"Hit: {hit.collider.name}, " +
                     $"trigger={hit.collider.isTrigger}, " +
                    $"distance={hit.distance:F2}"
                );
            
            if (Owner != null && hit.collider.transform.IsChildOf(Owner.transform))
                continue;

            ImpactEffectUtil.SpawnImpact(hit, ImpactKind.Melee, impactDatabase);

            IDamageable damageable = DamageUtil.FindDamageable(hit.collider);

            if (damageable != null)
            {
                damageable.TakeDamage(new DamageData
                {
                    amount = meleeData.damageAmount,
                    hitPoint = hit.point,
                    hitDirection = ray.direction,
                    source = Owner != null ? Owner.OwnerObject : gameObject,
                    damageType = meleeData.damageType
                });

                return;
            }

            if (!hit.collider.isTrigger)
            {
                GameLog.Log(TAG, $"Blocked by solid collider: {hit.collider.name}");
                return;
            }
        }
    }
}