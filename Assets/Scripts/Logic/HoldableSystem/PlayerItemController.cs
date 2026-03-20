using UnityEngine;

public class PlayerItemController : MonoBehaviour
{
    private const string TAG = "PlayerItemController";

    [Header("Links")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform aimOrigin;

    private Transform heldItemAnchor;

    public HeldSlot? CurrentSlot { get; private set; }
    public HoldableItemData EquippedData { get; private set; }
    public PlayerHeldItem CurrentHeldItem { get; private set; }

    public Transform AimOrigin => aimOrigin;
    public GameObject OwnerObject => gameObject;

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

        if (CurrentSlot == slot && EquippedData == data && CurrentHeldItem != null)
            return;

        EquipRuntime(slot.Value, data);
    }

    private void EquipRuntime(HeldSlot slot, HoldableItemData holdableData)
    {
        ResolveViewReferences();

        UnequipRuntime();

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