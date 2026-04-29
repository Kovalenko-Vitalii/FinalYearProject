using UnityEngine;

// This class represents foundation for any kind of item that can be holded in player`s hands
// It represents behabiour of hand held item
public abstract class PlayerHeldItem : MonoBehaviour
{
    public HoldableItemData Data { get; private set; }
    public InventoryItem ItemInstance { get; private set; }
    protected PlayerItemController Owner { get; private set; }

    [Header("View Model Root")]
    [SerializeField] protected Transform viewRoot; 

    [Header("Pose Settings")]
    [SerializeField] protected float poseLerpSpeed = 10f;

    [SerializeField] protected Transform idlePose;
    [SerializeField] protected Transform sprintPose;

    [SerializeField] protected AudioSource audioSource;

    protected Transform targetPose;

    protected virtual void Awake()
    {
        if (viewRoot == null)
            viewRoot = transform;
    }

    // (!!! need to move to playertick !!!)
    protected virtual void Update()
    {
        UpdatePose();
    }

    public virtual void Initialize(PlayerItemController owner, InventoryItem itemInstance)
    {
        Owner = owner;
        ItemInstance = itemInstance;
        Data = itemInstance?.data as HoldableItemData;
    }

    public virtual void OnEquip()
    {
        SnapToIdle();
    }

    public virtual void OnUnequip() { }

    public virtual void OnPrimaryPressed() { }
    public virtual void OnPrimaryReleased() { }

    public virtual void OnSecondaryPressed() { }
    public virtual void OnSecondaryReleased() { }

    public virtual void OnReloadPressed() { }
    public virtual void OnReloadReleased() { }

    public virtual void OnSprintStarted()
    {
        SetSprintPose();
    }

    public virtual void OnSprintStopped()
    {
        SetIdlePose();
    }

    protected void SetIdlePose()
    {
        if (idlePose != null)
            targetPose = idlePose;
    }

    protected void SetSprintPose()
    {
        if (sprintPose != null)
            targetPose = sprintPose;
    }

    protected void SnapToIdle()
    {
        if (idlePose == null)
            return;

        targetPose = idlePose;
        viewRoot.localPosition = idlePose.localPosition;
        viewRoot.localRotation = idlePose.localRotation;
    }

    protected virtual void UpdatePose()
    {
        if (targetPose == null)
            return;

        viewRoot.localPosition = Vector3.Lerp(
            viewRoot.localPosition,
            targetPose.localPosition,
            Time.deltaTime * poseLerpSpeed
        );

        viewRoot.localRotation = Quaternion.Slerp(
            viewRoot.localRotation,
            targetPose.localRotation,
            Time.deltaTime * poseLerpSpeed
        );
    }
}