using System;
using System.Collections;
using UnityEngine;

// This class represents runtime firearm holdable instance
public class HeldFirearmItem : PlayerHeldItem
{
    string TAG = "FIREARM";

    [SerializeField] private Transform aimPose;

    [Header("VFX")]
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private GameObject muzzleSmokePrefab;
    [SerializeField] private Transform muzzlePoint;

    [SerializeField] private ImpactEffectDatabase impactDatabase;

    public event Action<float> OnReloadProgressChanged;
    public event Action<bool> OnReloadStateChanged;

    private bool isAiming;
    private bool isSprinting;
    private bool reloadHeld;
    private float nextFireTime; 

    private Coroutine reloadRoutine;

    private HoldableFirearmData firearmData;
    private FirearmRuntimeState runtimeState;

    // === Initialization ===
    public override void Initialize(PlayerItemController owner, InventoryItem itemInstance)
    {
        base.Initialize(owner, itemInstance);

        firearmData = Data as HoldableFirearmData;
        if (firearmData == null)
        {
            GameLog.Error(TAG, $"[{nameof(HeldFirearmItem)}] Expected {nameof(HoldableFirearmData)}, got {Data?.GetType().Name}");
            return;
        }

        ItemInstance?.EnsureRuntimeState();
        runtimeState = ItemInstance?.firearmState;
    }
    public override void OnEquip()
    {
        base.OnEquip();

        isAiming = false;
        isSprinting = false;
        reloadHeld = false;
        nextFireTime = 0f;

        if (ItemInstance != null)
        {
            ItemInstance.EnsureRuntimeState();
            runtimeState = ItemInstance.firearmState;
        }

        NotifyReloadStopped();
    }
    public override void OnUnequip()
    {
        CancelReload();
    }

    // === Input callbacks ===
    public override void OnPrimaryPressed() {
        if (isSprinting || PauseManager.Instance.IsPaused)
            return;

        CancelReload();
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
        CancelReload();
        SetSprintPose();
    }
    public override void OnSprintStopped()
    {
        isSprinting = false;
        SetIdlePose();
    }
    public override void OnReloadPressed()
    {
        if (PauseManager.Instance.IsPaused || firearmData == null || runtimeState == null)
            return;

        reloadHeld = true;

        if (reloadRoutine == null && CanStartReload())
            reloadRoutine = StartCoroutine(ReloadRoutine());
    }
    public override void OnReloadReleased()
    {
        reloadHeld = false;

        if (firearmData != null && firearmData.reloadMode == ReloadMode.PerRound)
            CancelReload();
    }


    // === Firing Logic ===
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

        InventoryManager.Instance?.NotifyRuntimeItemStateChanged();

        nextFireTime = Time.time + firearmData.fireCooldown;

        PlaySound(firearmData.shotSound, AINoiseRanges.Gunshot);
        PlayMuzzleEffects();
        ApplyRecoil();
        PerformShot();
    }
    protected virtual bool HasAmmo()
    {
        return runtimeState != null && runtimeState.currentAmmoInMag >= firearmData.ammoPerShot;
    }
    protected virtual void ConsumeAmmo()
    {
        if (runtimeState == null)
            return;

        runtimeState.currentAmmoInMag = Mathf.Max(0, runtimeState.currentAmmoInMag - firearmData.ammoPerShot);
    }
    protected virtual void DryFire() => PlaySound(firearmData.dryShotSound, AINoiseRanges.MinorSound);
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

            ImpactEffectUtil.SpawnImpact(hit, ImpactKind.Bullet, impactDatabase);
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
        if (ParticleManager.Instance == null || muzzlePoint == null)
            return;

        if (muzzleFlashPrefab != null)
            ParticleManager.Instance.PlayOneShot(muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation);

        if (muzzleSmokePrefab != null)
            ParticleManager.Instance.PlayOneShot(muzzleSmokePrefab, muzzlePoint.position, muzzlePoint.rotation);
    }
    protected virtual void PlaySound(AudioClip clip, float radius)
    {
        if (clip != null)
        {
            SoundManager.Instance.PlayWorldOneShot(clip, transform.position);
            AIHearingReceiver.BroadcastNoise(transform.position, radius);
        }
    }
    
    // === Reload Logic ===
    private bool CanStartReload()
    {
        if (firearmData == null || runtimeState == null)
            return false;

        if (isSprinting)
            return false;

        if (runtimeState.currentAmmoInMag >= firearmData.magCapacity)
            return false;

        var im = InventoryManager.Instance;
        if (im == null || firearmData.ammoItem == null)
            return false;

        return im.HasPlayerItems(firearmData.ammoItem, 1);
    }
    private IEnumerator ReloadRoutine()
    {
        runtimeState.isReloading = true;
        OnReloadStateChanged?.Invoke(true);

        while (CanContinueReloadLoop())
        {
            float duration = Mathf.Max(0.01f, firearmData.reloadDuration);
            float t = 0f;

            

            while (t < duration)
            {
                if (!CanContinueCurrentReloadStep())
                {
                    NotifyReloadStopped();
                    reloadRoutine = null;
                    yield break;
                }

                t += Time.deltaTime;
                float progress = Mathf.Clamp01(t / duration);

                runtimeState.reloadProgress01 = progress;
                OnReloadProgressChanged?.Invoke(progress);

                yield return null;
            }

            bool inserted = InsertAmmo();
            if (!inserted)
            {
                NotifyReloadStopped();
                reloadRoutine = null;
                yield break;
            }

            PlaySound(firearmData.reloadSound, AINoiseRanges.MinorSound);

            runtimeState.reloadProgress01 = 0f;
            OnReloadProgressChanged?.Invoke(0f);

            if (firearmData.reloadMode == ReloadMode.Magazine)
                break;
        }

        NotifyReloadStopped();
        reloadRoutine = null;
    }
    private bool CanContinueReloadLoop()
    {
        if (!CanStartReload())
            return false;

        if (firearmData.reloadMode == ReloadMode.PerRound && !reloadHeld)
            return false;

        return true;
    }
    private bool CanContinueCurrentReloadStep()
    {
        if (PauseManager.Instance.IsPaused)
            return false;

        if (isSprinting)
            return false;

        if (!CanStartReload())
            return false;

        if (firearmData.reloadMode == ReloadMode.PerRound && !reloadHeld)
            return false;

        return true;
    }
    private bool InsertAmmo()
    {
        var im = InventoryManager.Instance;
        if (im == null || firearmData.ammoItem == null)
            return false;

        int missing = firearmData.magCapacity - runtimeState.currentAmmoInMag;
        if (missing <= 0)
            return false;

        if (firearmData.reloadMode == ReloadMode.Magazine)
        {
            int available = im.GetPlayerItemCount(firearmData.ammoItem);
            int toLoad = Mathf.Min(missing, available);

            if (toLoad <= 0)
                return false;

            if (!im.TryConsumePlayerItems(firearmData.ammoItem, toLoad))
                return false;

            runtimeState.currentAmmoInMag += toLoad;
            im.NotifyRuntimeItemStateChanged();
            return true;
        }

        // PerRound
        if (!im.TryConsumePlayerItems(firearmData.ammoItem, 1))
            return false;

        runtimeState.currentAmmoInMag += 1;
        im.NotifyRuntimeItemStateChanged();
        return true;
    }
    private void CancelReload()
    {
        reloadHeld = false;

        if (reloadRoutine != null)
        {
            StopCoroutine(reloadRoutine);
            reloadRoutine = null;
        }

        NotifyReloadStopped();
    }
    private void NotifyReloadStopped()
    {
        if (runtimeState != null)
        {
            runtimeState.isReloading = false;
            runtimeState.reloadProgress01 = 0f;
        }

        OnReloadProgressChanged?.Invoke(0f);
        OnReloadStateChanged?.Invoke(false);
    }
}