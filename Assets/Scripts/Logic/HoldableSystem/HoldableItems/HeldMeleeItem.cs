using System;
using UnityEngine;

public class HeldMeleeItem : PlayerHeldItem
{
    [Header("Melee Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string swingTriggerName = "Swing";

    [Header("Debug")]
    [SerializeField] private bool debugHitLogs = true;

    private bool _isBusy;
    private bool _isSprinting;
    private bool _isHoldingAttack;
    private bool _hitAppliedThisSwing;

    private static readonly int IsSprintingHash = Animator.StringToHash("IsSprinting");
    private int _swingTriggerHash;

    private HoldableMeleeData meleeData => Data as HoldableMeleeData;

    protected override void Awake()
    {
        base.Awake();

        _swingTriggerHash = Animator.StringToHash(swingTriggerName);

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    public override void OnEquip()
    {
        base.OnEquip();

        _isBusy = false;
        _isSprinting = false;
        _isHoldingAttack = false;
        _hitAppliedThisSwing = false;

        if (animator != null)
            animator.SetBool(IsSprintingHash, false);
    }

    public override void OnUnequip()
    {
        _isBusy = false;
        _isHoldingAttack = false;
        _hitAppliedThisSwing = false;
    }

    public override void OnPrimaryPressed()
    {
        if (_isSprinting)
            return;

        _isHoldingAttack = true;
        TryStartSwing();
    }

    public override void OnPrimaryReleased()
    {
        _isHoldingAttack = false;
    }

    public override void OnSprintStarted()
    {
        _isSprinting = true;
        _isHoldingAttack = false;
        SetSprintPose();

        if (animator != null)
            animator.SetBool(IsSprintingHash, true);
    }

    public override void OnSprintStopped()
    {
        _isSprinting = false;
        SetIdlePose();

        if (animator != null)
            animator.SetBool(IsSprintingHash, false);
    }

    private void TryStartSwing()
    {
        if (_isBusy || _isSprinting)
            return;

        _isBusy = true;
        _hitAppliedThisSwing = false;

        if (animator != null)
            animator.SetTrigger(_swingTriggerHash);
    }

    public void OnSwingHitFrame()
    {
        if (_hitAppliedThisSwing)
            return;

        _hitAppliedThisSwing = true;
        PerformHit();
    }

    public void OnSwingAnimationFinished()
    {
        _isBusy = false;

        if (_isHoldingAttack && !_isSprinting)
            TryStartSwing();
    }

    private void PerformHit()
    {
        Camera cam = HandsRig.Instance != null ? HandsRig.Instance.MainCamera : null;
        if (cam == null)
        {
            if (debugHitLogs) Debug.LogWarning("[HeldMeleeItem] MainCamera is null");
            return;
        }

        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f);
        Ray ray = cam.ScreenPointToRay(screenCenter);

        Debug.DrawRay(ray.origin, ray.direction * meleeData.hitDistance, Color.red, 1.5f);

        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            meleeData.hitDistance,
            meleeData.hitMask,
            QueryTriggerInteraction.Collide
        );

        if (hits == null || hits.Length == 0)
        {
            if (debugHitLogs) Debug.Log("[HeldMeleeItem] No hits");
            return;
        }

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            if (debugHitLogs)
            {
                Debug.Log(
                    $"[HeldMeleeItem] Hit: {hit.collider.name}, " +
                    $"trigger={hit.collider.isTrigger}, " +
                    $"distance={hit.distance:F2}"
                );
            }

            if (Owner != null && hit.collider.transform.IsChildOf(Owner.transform))
                continue;

            IDamageable damageable = FindDamageable(hit.collider);

            if (damageable != null)
            {
                if (debugHitLogs)
                    Debug.Log($"[HeldMeleeItem] Applying damage to {((MonoBehaviour)damageable).name}");

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
                if (debugHitLogs)
                    Debug.Log($"[HeldMeleeItem] Blocked by solid collider: {hit.collider.name}");
                return;
            }
        }
    }

    private IDamageable FindDamageable(Collider col)
    {
        MonoBehaviour[] behaviours = col.GetComponentsInParent<MonoBehaviour>(true);

        foreach (var behaviour in behaviours)
        {
            if (behaviour is IDamageable damageable)
                return damageable;
        }

        return null;
    }
}