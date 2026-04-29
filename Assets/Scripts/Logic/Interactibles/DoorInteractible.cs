using System;
using UnityEngine;

// This class represents interactible door
public class DoorInteractable : MonoBehaviour, IInteractable, ISaveable
{
    [SerializeField] string id;
    [SerializeField] ItemData key;

    [SerializeField] private QuestGate questGate;

    [Header("Side Access")]
    [SerializeField] private DoorSideAccess sideAccess = DoorSideAccess.BothSides;
    [SerializeField] private Transform sideReference;
    [SerializeField, Range(0f, 0.3f)] private float sideDeadZone = 0.05f;
    [SerializeField] private string wrongSidePrompt = "Locked from other side";
    public bool isOpen { get; private set; }

    [Header("UX")]
    [SerializeField] private string displayName = "Open [E]";

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
        switch (GetBlockReason(interactor))
        {
            case DoorBlockReason.QuestBlocked:
                prompt = questGate.LockedPrompt;
                return true;

            case DoorBlockReason.WrongSide:
                prompt = wrongSidePrompt;
                return true;

            case DoorBlockReason.Locked:
                prompt = $"Open {displayName} (Locked)";
                return true;

            default:
                prompt = isOpen ? $"Close {displayName}" : $"Open {displayName}";
                return true;
        }
    }

    public bool Interact(PlayerInteractor interactor)
    {
        var blockReason = GetBlockReason(interactor);

        switch (blockReason)
        {
            case DoorBlockReason.QuestBlocked:
            case DoorBlockReason.WrongSide:
            case DoorBlockReason.Locked:
                PlayLockedTry();
                PlayLockedSound();
                return false;
        }

        if (isLocked && !isOpen)
        {
            if (key != null)
            {
                bool hasKey = interactor.PlayerInventory.HasItemById(key.id);
                if (hasKey)
                {
                    PlayUnlockSound();
                    isLocked = false;
                    SetOpen(true);
                    return true;
                }
            }
            else if (sideAccess != DoorSideAccess.BothSides)
            {
                isLocked = false;
                SetOpen(true);
                return true;
            }
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

    private bool CanOpenFromThisSide(PlayerInteractor interactor)
    {
        if (isOpen)
            return true;

        if (sideAccess == DoorSideAccess.BothSides)
            return true;

        if (!isLocked)
            return true;

        Transform refTransform = sideReference != null ? sideReference : transform;

        Vector3 forward = Vector3.ProjectOnPlane(refTransform.forward, Vector3.up).normalized;
        Vector3 toPlayer = Vector3.ProjectOnPlane(interactor.transform.position - refTransform.position, Vector3.up);

        if (toPlayer.sqrMagnitude < 0.0001f || forward.sqrMagnitude < 0.0001f)
            return true;

        toPlayer.Normalize();

        float dot = Vector3.Dot(forward, toPlayer);

        if (Mathf.Abs(dot) <= sideDeadZone)
            return true;

        return sideAccess switch
        {
            DoorSideAccess.FrontOnlyWhenClosed => dot > 0f,
            DoorSideAccess.BackOnlyWhenClosed => dot < 0f,
            _ => true
        };
    }

    private DoorBlockReason GetBlockReason(PlayerInteractor interactor)
    {
        if (questGate != null && !questGate.IsPassed())
            return DoorBlockReason.QuestBlocked;

        if (!CanOpenFromThisSide(interactor))
            return DoorBlockReason.WrongSide;

        if (isLocked && !isOpen)
        {
            if (sideAccess != DoorSideAccess.BothSides && key == null)
                return DoorBlockReason.None;

            bool hasKey = key != null && interactor.PlayerInventory.HasItemById(key.id);
            if (!hasKey)
                return DoorBlockReason.Locked;
        }

        return DoorBlockReason.None;
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

    public void ResetToDefaultState()
    {

    }
}

[Serializable]
public struct DoorState
{
    public bool isOpen;
    public bool isLocked;
}

public enum DoorSideAccess
{
    BothSides,
    FrontOnlyWhenClosed,
    BackOnlyWhenClosed
}

public enum DoorBlockReason
{
    None,
    QuestBlocked,
    WrongSide,
    Locked
}