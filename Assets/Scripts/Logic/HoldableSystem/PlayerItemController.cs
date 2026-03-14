using UnityEngine;

public class PlayerItemController : MonoBehaviour
{
    string TAG = "PlayerControllerManager";
    [Header("Links")]
    [SerializeField] private PlayerMovement playerMovement;
    Transform heldItemAnchor;

    public HeldSlot? CurrentSlot { get; private set; }
    public HoldableItemData EquippedData { get; private set; }
    public PlayerHeldItem CurrentHeldItem { get; private set; }

    private bool _wasSprintingLastFrame;

    private void Awake()
    {
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnActiveHeldSlotChanged += HandleActiveHeldSlotChanged;
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnActiveHeldSlotChanged -= HandleActiveHeldSlotChanged;
    }

    private void Start()
    {
        var im = InventoryManager.Instance;
        if (im != null)
            SyncFromManager(im.ActiveHeldSlot);

        if (heldItemAnchor == null && HandsRig.Instance != null)
            heldItemAnchor = HandsRig.Instance.HeldItemAnchor;
    }

    private void Update()
    {
        HandleInput();
        HandleSprintState();
    }

    private void HandleActiveHeldSlotChanged(HeldSlot? slot)
    {
        SyncFromManager(slot);
    }

    private void SyncFromManager(HeldSlot? slot)
    {
        var im = InventoryManager.Instance;
        if (im == null)
            return;

        if (!slot.HasValue)
        {
            UnequipRuntime();
            return;
        }

        var data = im.playerHeldEquipment.GetEquipped(slot.Value);
        if (data == null)
        {
            UnequipRuntime();
            return;
        }

        EquipRuntime(slot.Value, data);
    }

    private void EquipRuntime(HeldSlot slot, HoldableItemData holdableData)
    {
        UnequipRuntime();

        if (holdableData.firstPersonPrefab == null)
        {
            GameLog.Log(TAG, $"No firstPersonPrefab on {holdableData.name}");
            return;
        }

        GameObject instance = Instantiate(holdableData.firstPersonPrefab, heldItemAnchor);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;

        PlayerHeldItem heldItem = instance.GetComponent<PlayerHeldItem>();
        if (heldItem == null)
        {
            GameLog.Log(TAG, $"[PlayerItemController] Prefab {holdableData.firstPersonPrefab.name} has no PlayerHeldItem.");
            Destroy(instance);
            return;
        }

        CurrentSlot = slot;
        EquippedData = holdableData;
        CurrentHeldItem = heldItem;

        CurrentHeldItem.Initialize(this, holdableData);
        CurrentHeldItem.OnEquip();
    }

    private void UnequipRuntime()
    {
        if (CurrentHeldItem != null)
        {
            CurrentHeldItem.OnUnequip();
            Destroy(CurrentHeldItem.gameObject);
        }

        CurrentSlot = null;
        EquippedData = null;
        CurrentHeldItem = null;
    }

    private void HandleInput()
    {
        var im = InventoryManager.Instance;
        if (im != null)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                im.ToggleActiveHeldSlot(HeldSlot.Slot1);

            if (Input.GetKeyDown(KeyCode.Alpha2))
                im.ToggleActiveHeldSlot(HeldSlot.Slot2);
        }

        if (CurrentHeldItem == null)
            return;

        if (Input.GetMouseButtonDown(0))
            CurrentHeldItem.OnPrimaryPressed();

        if (Input.GetMouseButtonUp(0))
            CurrentHeldItem.OnPrimaryReleased();

        if (Input.GetMouseButtonDown(1))
            CurrentHeldItem.OnSecondaryPressed();

        if (Input.GetMouseButtonUp(1))
            CurrentHeldItem.OnSecondaryReleased();

        if (Input.GetKeyDown(KeyCode.R))
            CurrentHeldItem.OnReloadPressed();
    }

    private void HandleSprintState()
    {
        bool isSprinting = playerMovement != null && playerMovement.IsSprinting;

        if (CurrentHeldItem == null)
        {
            _wasSprintingLastFrame = isSprinting;
            return;
        }

        if (isSprinting && !_wasSprintingLastFrame)
            CurrentHeldItem.OnSprintStarted();

        if (!isSprinting && _wasSprintingLastFrame)
            CurrentHeldItem.OnSprintStopped();

        _wasSprintingLastFrame = isSprinting;
    }
}