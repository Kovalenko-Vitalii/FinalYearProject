using UnityEngine;

public class HeldFirearmItem : PlayerHeldItem
{
    string TAG = "FIREARM";

    [SerializeField] private Transform aimPose;

    [Header("VFX")]
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private ParticleSystem muzzleFlashFx;
    [SerializeField] private ParticleSystem muzzleSmokeFx;
    [SerializeField] private ImpactEffectDatabase impactDatabase;

    private bool isAiming;
    private bool isSprinting;
    private float nextFireTime;

    private HoldableFirearmData firearmData;

    public override void Initialize(PlayerItemController owner, HoldableItemData data)
    {
        base.Initialize(owner, data);

        firearmData = data as HoldableFirearmData;
        if (firearmData == null)
        {
            GameLog.Error(TAG, $"[{nameof(HeldFirearmItem)}] Expected {nameof(HoldableFirearmData)}, got {data?.GetType().Name}");
            return;
        }
    }

    public override void OnEquip()
    {
        base.OnEquip();
        isAiming = false;
        isSprinting = false;
        nextFireTime = 0f;
    }

    public override void OnPrimaryPressed() {
        if (isSprinting || PauseManager.Instance.IsPaused)
            return;

        TryFire(); 
    }

    public override void OnSecondaryPressed()
    {
        if (isSprinting)
            return;

        isAiming = true;
        SetAimPose();
    }

    public override void OnSecondaryReleased()
    {
        isAiming = false;
        SetIdlePose();
    }

    public override void OnSprintStarted()
    {
        isSprinting = true;
        isAiming = false;
        SetSprintPose();
    }

    public override void OnSprintStopped()
    {
        isSprinting = false;
        SetIdlePose();
    }

    public override void OnReloadPressed() { }

    
    protected virtual void TryFire()
    {
        if (firearmData == null)
            return;

        if (isSprinting)
            return;

        if (Time.time < nextFireTime)
            return;

        if (!HasAmmo())
        {
            DryFire();
            nextFireTime = Time.time + firearmData.fireCooldown;
            return;
        }

        ConsumeAmmo();
        nextFireTime = Time.time + firearmData.fireCooldown;

        PlaySound(firearmData.shotSound);
        PlayMuzzleEffects();
        ApplyRecoil();
        PerformShot();
    }

    protected virtual bool HasAmmo()
    {
        var im = InventoryManager.Instance;
        if (im == null || firearmData.ammoItem == null)
            return false;

        return im.HasPlayerItems(firearmData.ammoItem, firearmData.ammoPerShot);
    }

    protected virtual void ConsumeAmmo()
    {
        var im = InventoryManager.Instance;
        if (im == null || firearmData.ammoItem == null)
            return;

        im.TryConsumePlayerItems(firearmData.ammoItem, firearmData.ammoPerShot);
    }

    protected virtual void DryFire() => PlaySound(firearmData.dryShotSound);
    
    protected virtual void PerformShot()
    {
        if (Owner == null || !Owner.TryGetAimRay(out Ray ray))
        {
            GameLog.Warning(TAG, "Aim ray is unavailable");
            return;
        }

        Debug.DrawRay(ray.origin, ray.direction * firearmData.fireDistance, Color.yellow, 1.5f);

        if (Physics.Raycast(ray, out RaycastHit hit, firearmData.fireDistance, firearmData.hitMask, QueryTriggerInteraction.Collide))
        {
            if (Owner != null && hit.collider.transform.IsChildOf(Owner.transform))
                return;

            ImpactEffectUtil.SpawnImpact(hit, ImpactKind.Bullet, impactDatabase);

            IDamageable damageable = DamageUtil.FindDamageable(hit.collider);
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
            GameLog.Log(TAG, $" Hit {hit.collider.name}");
        }
    }

    protected virtual void ApplyRecoil()
    {
        viewRoot.localPosition -= new Vector3(0f, firearmData.fireKickBack, 0f);
        viewRoot.localRotation *= Quaternion.Euler(0f, 0f, firearmData.fireKickUp);
    }

    private void SetAimPose()
    {
        if (aimPose != null)
            targetPose = aimPose;
    }

    protected virtual void PlayMuzzleEffects()
    {
        if (muzzleFlashFx != null)
            muzzleFlashFx.Play();

        if (muzzleSmokeFx != null)
            muzzleSmokeFx.Play();
    }

    protected virtual void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}