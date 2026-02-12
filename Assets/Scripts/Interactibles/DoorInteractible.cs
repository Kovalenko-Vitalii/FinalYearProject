using System;
using UnityEngine;

public class DoorInteractable : MonoBehaviour, IInteractable, IHoldInteractable, IHoldFeedback
{
    [Header("UX")]
    [SerializeField] private string displayName = "Door";

    [Header("Hold Settings")]
    [SerializeField] private float openHoldDuration = 0.35f;
    [SerializeField] private float closeHoldDuration = 0.15f;
    [SerializeField] private float lockedTryDuration = 0.15f; 

    [Header("Lock")]
    [SerializeField] private bool isLocked = false;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string isOpenBoolName = "isOpen";
    [SerializeField] private string tryLockedTriggerName = "tryLocked";

    [Header("Sound")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip openSound;
    [SerializeField] AudioClip closeSound;
    [SerializeField] AudioClip lockedSound;
    [SerializeField] AudioClip unlockSound;

    [Header("Save")]
    [SerializeField] private string id;
    [SerializeField] private bool isOpen;
    [SerializeField] ItemData key;
    private int isOpenHash;
    private int tryLockedHash;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
        {
            GenerateId();
            return;
        }

        var all = FindObjectsByType<DoorInteractable>(FindObjectsSortMode.None);
        foreach (var c in all)
        {
            if (c == this) continue;
            if (c.id == id)
            {
                GenerateId();
                break;
            }
        }
    }

    private void GenerateId()
    {
        id = System.Guid.NewGuid().ToString("N");
        UnityEditor.EditorUtility.SetDirty(this);
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

    // --- Public API for save/load
    public string Id => id;
    public bool IsOpen => isOpen;
    public bool IsLocked => isLocked;

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
            bool hasKey = key != null && interactor != null && interactor.PlayerInventory != null
                          && interactor.PlayerInventory.HasItemById(key.id);

            if (!hasKey)
                PlayLockedTry();
        }
    }


    public void OnHoldCanceled(PlayerInteractor interactor)
    {

    }

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

    // Sound stuff
    public void PlayOpenSound()
    {
        if (audioSource && openSound)
            audioSource.PlayOneShot(openSound);
    }

    public void PlayCloseSound()
    {
        if (audioSource && closeSound)
            audioSource.PlayOneShot(closeSound);
    }

    public void PlayLockedSound()
    {
        if (audioSource && lockedSound)
            audioSource.PlayOneShot(lockedSound);
    }

    public void PlayUnlockSound()
    {
        if (audioSource && unlockSound)
            audioSource.PlayOneShot(unlockSound);
    }
}
