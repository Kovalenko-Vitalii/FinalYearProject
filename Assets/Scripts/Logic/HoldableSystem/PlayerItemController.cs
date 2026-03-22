using UnityEngine;

public class PlayerItemController : MonoBehaviour
{
    private const string TAG = "PlayerItemController";

    [Header("Links")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform aimOrigin;

    private Transform heldItemAnchor;

    public EquipmentSlotId? CurrentSlot { get; private set; }
    public HoldableItemData EquippedData { get; private set; }
    public PlayerHeldItem CurrentHeldItem { get; private set; }

    public Transform AimOrigin => aimOrigin;
    public GameObject OwnerObject => gameObject;

    private bool wasSprintingLastFrame;

    // === Unity Lifecycle ===
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

        UnequipRuntime();
    }
    private void Start()
    {
        ResolveViewReferences();

        var im = InventoryManager.Instance;
        if (im != null)
            SyncFromManager(im.ActiveHeldSlot);
    }
    private void Update()
    {
        HandleInput();
        HandleSprintState();
    }

    // === Sync from Manager ===
    private void HandleActiveHeldSlotChanged(EquipmentSlotId? slot)
    {
        SyncFromManager(slot);
    }
    private void SyncFromManager(EquipmentSlotId? slot)
    {
        var im = InventoryManager.Instance;
        if (im == null)
            return;

        if (!slot.HasValue)
        {
            UnequipRuntime();
            return;
        }

        var item = im.GetEquippedItem(slot.Value);
        if (item == null || item.data is not HoldableItemData)
        {
            UnequipRuntime();
            return;
        }

        if (CurrentSlot == slot && CurrentHeldItem != null && CurrentHeldItem.ItemInstance == item)
            return;

        EquipRuntime(slot.Value, item);
    }

    // === Runtime equip/unequip ===
    private void EquipRuntime(EquipmentSlotId? slot, InventoryItem item)
    {
        ResolveViewReferences();
        UnequipRuntime();

        if (item == null || item.data is not HoldableItemData holdableData)
            return;

        if (heldItemAnchor == null)
        {
            GameLog.Log(TAG, "Held item anchor is missing.");
            return;
        }

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
            GameLog.Log(TAG, $"Prefab {holdableData.firstPersonPrefab.name} has no PlayerHeldItem.");
            Destroy(instance);
            return;
        }

        CurrentSlot = slot;
        EquippedData = holdableData;
        CurrentHeldItem = heldItem;

        CurrentHeldItem.Initialize(this, item);
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

    // === Handling Input ===
    private void HandleInput()
    {
        var im = InventoryManager.Instance;
        if (im != null)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                im.ToggleActiveHeldSlot(EquipmentSlotId.Held1);

            if (Input.GetKeyDown(KeyCode.Alpha2))
                im.ToggleActiveHeldSlot(EquipmentSlotId.Held2);
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

        if (Input.GetKeyUp(KeyCode.R))
            CurrentHeldItem.OnReloadReleased();
    }
    private void HandleSprintState()
    {
        bool isSprinting = playerMovement != null && playerMovement.IsSprinting;

        if (CurrentHeldItem == null)
        {
            wasSprintingLastFrame = isSprinting;
            return;
        }

        if (isSprinting && !wasSprintingLastFrame)
            CurrentHeldItem.OnSprintStarted();

        if (!isSprinting && wasSprintingLastFrame)
            CurrentHeldItem.OnSprintStopped();

        wasSprintingLastFrame = isSprinting;
    }

    // === Helpers ===
    private void ResolveViewReferences()
    {
        if (HandsRig.Instance != null)
        {
            if (heldItemAnchor == null)
                heldItemAnchor = HandsRig.Instance.HeldItemAnchor;

            if (aimOrigin == null && HandsRig.Instance.MainCamera != null)
                aimOrigin = HandsRig.Instance.MainCamera.transform;
        }
    }
    public bool TryGetAimRay(out Ray ray)
    {
        ResolveViewReferences();

        if (aimOrigin == null)
        {
            ray = default;
            return false;
        }

        ray = new Ray(aimOrigin.position, aimOrigin.forward);
        return true;
    }
}