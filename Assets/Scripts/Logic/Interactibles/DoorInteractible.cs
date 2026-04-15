using System;
using UnityEngine;

public class DoorInteractable : MonoBehaviour, IInteractable, ISaveable
{
    [SerializeField] string id;
    [SerializeField] ItemData key;
    [SerializeField] private QuestGate questGate;
    public bool isOpen { get; private set; }

    [Header("UX")]
    [SerializeField] private string displayName = "Door";

    [Header("Lock")]
    public bool isLocked;

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

        if (questGate == null)
            questGate = GetComponent<QuestGate>();

        isOpenHash = !string.IsNullOrEmpty(isOpenBoolName) ? Animator.StringToHash(isOpenBoolName) : 0;
        tryLockedHash = !string.IsNullOrEmpty(tryLockedTriggerName) ? Animator.StringToHash(tryLockedTriggerName) : 0;

        ApplyStateImmediate(isOpen, isLocked);
    }



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
        if (questGate != null && !questGate.IsPassed())
        {
            prompt = questGate.LockedPrompt;
            return true;
        }

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
        if (questGate != null && !questGate.IsPassed())
            return false;

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
    public void PlayOpenSound() {
        if (openSound) {
            SoundManager.Instance.PlayWorldOneShot(openSound, transform.position);
            AIHearingReceiver.BroadcastNoise(transform.position, AINoiseRanges.DoorSound);
        }
    }
    public void PlayCloseSound() {
        if (closeSound) {
            SoundManager.Instance.PlayWorldOneShot(closeSound, transform.position);
            AIHearingReceiver.BroadcastNoise(transform.position, AINoiseRanges.DoorSound);
        }
    }
    public void PlayLockedSound() { 
        if (lockedSound) { 
            SoundManager.Instance.PlayWorldOneShot(lockedSound, transform.position); 
            AIHearingReceiver.BroadcastNoise(transform.position, AINoiseRanges.DoorSound); 
        }
    }
    public void PlayUnlockSound() {
        if (unlockSound) {
            SoundManager.Instance.PlayWorldOneShot(unlockSound, transform.position); 
            AIHearingReceiver.BroadcastNoise(transform.position, AINoiseRanges.DoorSound); 
        } 
    }

    // --- ISaveable
    public object CaptureState() => new DoorState { isOpen = isOpen, isLocked = isLocked };
}

[Serializable]
public struct DoorState
{
    public bool isOpen;
    public bool isLocked;
}