using System;
using UnityEngine;

public class DoorInteractable : MonoBehaviour, IInteractable, IHoldInteractable, IHoldFeedback, ISaveable
{
    [SerializeField] string id;
    [SerializeField] ItemData key;
    public bool isOpen { get; private set; }

    [Header("UX")]
    [SerializeField] private string displayName = "Door";

    [Header("Hold Settings")]
    [SerializeField] private float openHoldDuration = 0.35f;
    [SerializeField] private float closeHoldDuration = 0.15f;
    [SerializeField] private float lockedTryDuration = 0.15f;

    [Header("Lock")]
    public bool isLocked { get; private set; }

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string isOpenBoolName = "isOpen";
    [SerializeField] private string tryLockedTriggerName = "tryLocked";

    [Header("Sound")]
    [SerializeField] AudioClip openSound;
    [SerializeField] AudioClip closeSound;
    [SerializeField] AudioClip lockedSound;
    [SerializeField] AudioClip unlockSound;

    private int isOpenHash;
    private int tryLockedHash;
    public string SaveId => id;

    private void Reset()
    {
#if UNITY_EDITOR
        SaveIdUtil.EnsureId(ref id, this);
#else
        if (string.IsNullOrWhiteSpace(id))
            id = Guid.NewGuid().ToString("N");
#endif
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        SaveIdUtil.EnsureId(ref id, this);
    }
#endif

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        isOpenHash = !string.IsNullOrEmpty(isOpenBoolName) ? Animator.StringToHash(isOpenBoolName) : 0;
        tryLockedHash = !string.IsNullOrEmpty(tryLockedTriggerName) ? Animator.StringToHash(tryLockedTriggerName) : 0;

        ApplyStateImmediate(isOpen, isLocked);
    }

    // --- ISaveable
    public object CaptureState() => new DoorState { isOpen = isOpen, isLocked = isLocked };

    public void RestoreState(object state)
    {
        if (state is not DoorState s) return;
        ApplyStateImmediate(s.isOpen, s.isLocked);
    }

    public void ApplyStateImmediate(bool open, bool locked)
    {
        isOpen = open;
        isLocked = locked;

        if (animator != null && isOpenHash != 0)
            animator.SetBool(isOpenHash, isOpen);

        if (animator != null && tryLockedHash != 0)
            animator.ResetTrigger(tryLockedHash);

        if (animator != null)
            animator.Update(0f);
    }

    public void SetLocked(bool locked) => isLocked = locked;

    // --- IInteractable
    public bool TryGetPrompt(PlayerInteractor interactor, out string prompt)
    {
        if (isLocked && !isOpen)
        {
            prompt = $"Open {displayName} (Locked)";
            return true;
        }

        prompt = isOpen ? $"Close {displayName}" : $"Open {displayName}";
        return true;
    }

    public bool Interact(PlayerInteractor interactor)
    {
        if (isLocked && !isOpen)
        {
            if (key != null && InventoryManager.Instance.playerInventory.HasItemById(key.id))
            {
                PlayUnlockSound();
                isLocked = false;
                return false;
            }

            PlayLockedTry();
            return false;
        }

        SetOpen(!isOpen);
        return true;
    }

    public float GetInteractDuration(PlayerInteractor interactor)
    {
        if (isLocked && !isOpen) return lockedTryDuration;
        return isOpen ? closeHoldDuration : openHoldDuration;
    }

    public void OnHoldStart(PlayerInteractor interactor, float duration)
    {
        if (isLocked && !isOpen)
        {
            bool hasKey = key != null && interactor?.PlayerInventory != null
                          && interactor.PlayerInventory.HasItemById(key.id);

            if (!hasKey)
                PlayLockedTry();
        }
    }

    public void OnHoldCanceled(PlayerInteractor interactor) { }

    private void SetOpen(bool value)
    {
        isOpen = value;

        if (animator != null && isOpenHash != 0)
            animator.SetBool(isOpenHash, isOpen);
    }

    private void PlayLockedTry()
    {
        if (animator == null || tryLockedHash == 0) return;

        animator.ResetTrigger(tryLockedHash);
        animator.SetTrigger(tryLockedHash);
    }

    // Sound
    public void PlayOpenSound() { if (openSound) SoundManager.Instance.PlayWorldOneShot(openSound, transform.position); }
    public void PlayCloseSound() { if (closeSound) SoundManager.Instance.PlayWorldOneShot(closeSound, transform.position); }
    public void PlayLockedSound() { if (lockedSound) SoundManager.Instance.PlayWorldOneShot(lockedSound, transform.position); }
    public void PlayUnlockSound() { if (unlockSound) SoundManager.Instance.PlayWorldOneShot(unlockSound, transform.position); }
}

[Serializable]
public struct DoorState
{
    public bool isOpen;
    public bool isLocked;
}