using UnityEngine;

public class HeldFirearmItem : PlayerHeldItem
{
    [Header("Aim")]
    [SerializeField] private Transform aimPose; 

    [SerializeField] private bool infiniteAmmo = false;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    [SerializeField] AudioSource audioSource;

    private bool _isAiming;
    private bool _isSprinting;
    private float _nextFireTime;

    private HoldableFirearmData firearmData => Data as HoldableFirearmData;

    public override void OnEquip()
    {
        base.OnEquip();
        _isAiming = false;
        _isSprinting = false;
        _nextFireTime = 0f;
    }

    public override void OnPrimaryPressed()
    {
        TryFire();
    }

    public override void OnSecondaryPressed()
    {
        if (_isSprinting)
            return;

        _isAiming = true;
        SetAimPose();
    }

    public override void OnSecondaryReleased()
    {
        _isAiming = false;
        SetIdlePose();
    }

    public override void OnSprintStarted()
    {
        _isSprinting = true;
        _isAiming = false;
        SetSprintPose();
    }

    public override void OnSprintStopped()
    {
        _isSprinting = false;
        SetIdlePose();
    }

    public override void OnReloadPressed()
    {
    }

    protected virtual void TryFire()
    {
        if (_isSprinting)
            return;

        if (Time.time < _nextFireTime)
            return;

        if (!HasAmmo())
        {
            DryFire();
            _nextFireTime = Time.time + firearmData.fireCooldown;
            return;
        }

        ConsumeAmmo();
        _nextFireTime = Time.time + firearmData.fireCooldown;

        ApplyRecoil();
        PerformShot();
    }

    protected virtual bool HasAmmo()
    {
        if (infiniteAmmo)
            return true;

        var im = InventoryManager.Instance;
        if (im == null || firearmData.ammoItem == null)
            return false;

        return im.HasPlayerItems(firearmData.ammoItem, firearmData.ammoPerShot);
    }

    protected virtual void ConsumeAmmo()
    {
        if (infiniteAmmo)
            return;

        var im = InventoryManager.Instance;
        if (im == null || firearmData.ammoItem == null)
            return;

        im.TryConsumePlayerItems(firearmData.ammoItem, firearmData.ammoPerShot);
    }

    protected virtual void DryFire()
    {
        if (debugLogs)
            Debug.Log("[HeldFirearmItem] Dry fire");
    }

    protected virtual void PerformShot()
    {
        if (Owner == null || !Owner.TryGetAimRay(out Ray ray))
        {
            if (debugLogs)
                Debug.LogWarning("[HeldFirearmItem] No aim ray");
            return;
        }

        Debug.DrawRay(ray.origin, ray.direction * firearmData.fireDistance, Color.yellow, 1.5f);

        if (Physics.Raycast(ray, out RaycastHit hit, firearmData.fireDistance, firearmData.hitMask, QueryTriggerInteraction.Collide))
        {
            if (Owner != null && hit.collider.transform.IsChildOf(Owner.transform))
                return;

            IDamageable damageable = FindDamageable(hit.collider);
            if (damageable != null)
            {
                damageable.TakeDamage(new DamageData
                {
                    amount = firearmData.damageAmount,
                    hitPoint = hit.point,
                    hitDirection = ray.direction,
                    source = Owner.OwnerObject,
                    damageType = firearmData.damageType
                });
            }

            audioSource.PlayOneShot(firearmData.shotSound);
            if (debugLogs)
                Debug.Log($"[HeldFirearmItem] Hit {hit.collider.name}");
        }
        else
        {
            if (debugLogs)
                Debug.Log("[HeldFirearmItem] Miss");
        }
    }

    protected virtual void ApplyRecoil()
    {
        viewRoot.localPosition -= new Vector3(0f, firearmData.fireKickBack, 0f);
        viewRoot.localRotation *= Quaternion.Euler(0f, 0f, firearmData.fireKickUp);
    }

    protected IDamageable FindDamageable(Collider col)
    {
        MonoBehaviour[] behaviours = col.GetComponentsInParent<MonoBehaviour>(true);

        foreach (var behaviour in behaviours)
        {
            if (behaviour is IDamageable damageable)
                return damageable;
        }

        return null;
    }

    private void SetAimPose()
    {
        if (aimPose != null)
            targetPose = aimPose;
    }
}