using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WorldContainer))]
public class WorldContainerInteractable : MonoBehaviour, IInteractable, IHoldInteractable, IHoldFeedback, ISaveable
{
    [Header("UX")]
    [SerializeField] private string displayName = "Box";

    [Header("Search Settings")]
    [SerializeField] private float firstSearchDuration = 1.5f;
    [SerializeField] private float quickOpenDuration = 0.1f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string isOpenBoolName = "isOpen";

    [Header("Sound")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    private bool isOpen = false;
    private bool isSearched = false;

    private WorldContainer container;

    public string SaveId => container != null ? container.Id : string.Empty;

    private int isOpenHash;

    private void Awake()
    {
        container = GetComponent<WorldContainer>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        isOpenHash = (!string.IsNullOrEmpty(isOpenBoolName)) ? Animator.StringToHash(isOpenBoolName) : 0;

        ApplyOpenImmediate(isOpen);
    }

    private void ApplyOpenImmediate(bool value)
    {
        isOpen = value;
        if (animator != null && isOpenHash != 0)
        {
            animator.SetBool(isOpenHash, isOpen);
            animator.Update(0f);
        }
    }

    public object CaptureState()
    {
        return new WorldContainerFullState
        {
            isSearched = isSearched,
            items = CaptureContent()
        };
    }

    public void RestoreState(object state)
    {
        if (state is not WorldContainerFullState s) return;

        isSearched = s.isSearched;

        RestoreContent(s.items);
    }

    private List<InventoryItemSave> CaptureContent()
    {
        var list = new List<InventoryItemSave>();

        if (container?.Inventory?.items == null)
            return list;

        foreach (var it in container.Inventory.items)
        {
            if (it == null || it.data == null || it.amount <= 0) continue;

            list.Add(new InventoryItemSave
            {
                itemId = it.data.id,
                amount = it.amount,
                durability = it.currentDurability
            });
        }

        return list;
    }

    private void RestoreContent(List<InventoryItemSave> items)
    {
        if (container == null) return;

        if (container.Inventory == null)
            return;

        container.Inventory.items.Clear();

        if (items == null) return;

        foreach (var it in items)
        {
            if (string.IsNullOrWhiteSpace(it.itemId) || it.amount <= 0) continue;

            var data = ItemResolver.Resolve(it.itemId);
            if (data == null) continue;

            container.Inventory.AddItem(data, it.amount, it.durability);
        }
    }

    // ----------------- INTERACT -----------------

    public bool TryGetPrompt(PlayerInteractor interactor, out string prompt)
    {
        prompt = isSearched ? $"Open {displayName}" : $"Search {displayName}";
        return true;
    }

    public float GetInteractDuration(PlayerInteractor interactor)
    {
        return isSearched ? quickOpenDuration : firstSearchDuration;
    }

    public bool Interact(PlayerInteractor interactor)
    {
        if (container == null)
            return false;

        var ui = CanvasSwitcher.Instance;
        if (ui == null)
            return false;

        var containerRoot = ui.Container.Root;
        var containerUI = containerRoot.GetComponent<ContainerUI>();
        if (containerUI == null)
            return false;

        containerUI.ShowFor(container, this);
        ui.TryOpenContainerMain();

        isSearched = true;
        return true;
    }

    public void OnHoldStart(PlayerInteractor interactor, float duration)
    {
        PlayOpenSound();
        ApplyOpenImmediate(true);
    }

    public void OnHoldCanceled(PlayerInteractor interactor)
    {
        if (!CanvasSwitcher.Instance.AnyOpen)
            ApplyOpenImmediate(false);
    }

    public void CloseLid()
    {
        PlayCloseSound();
        ApplyOpenImmediate(false);
    }

    // Sound stuff
    public void PlayOpenSound()
    {
        if (openSound)
            SoundManager.Instance.PlayWorldOneShot(openSound, transform.position);
    }

    public void PlayCloseSound()
    {
        if (closeSound)
            SoundManager.Instance.PlayWorldOneShot(closeSound, transform.position);
    }
}

[Serializable]
public struct WorldContainerFullState
{
    public bool isSearched;
    public List<InventoryItemSave> items;
}