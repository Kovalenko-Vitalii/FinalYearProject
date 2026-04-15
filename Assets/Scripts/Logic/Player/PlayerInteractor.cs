using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private float useDistance = 3f;
    [SerializeField] private LayerMask interactMask = ~0;

    [Header("Input")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Refs")]
    [SerializeField] private Camera cam;
    [SerializeField] private FPSUIManager ui;

    private IInteractable current;

    private IInteractable holdTarget;
    private float holdTimer;
    private float holdDuration;

    private void Awake() 
    {
        if (!cam) cam = Camera.main;
    }

    private void Update()
    {
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
        {
            if (current != null)
            {
                current = null;
                ResetHold();
                if (ui) ui.SetUnderCrosshairLabel("");
            }
            return;
        }

        UpdateHover();
        HandleInteractInput();
    }

    private void UpdateHover()
    {
        string label = "";

        var newCurrent = RaycastForInteractable();
        if (newCurrent != current)
        {
            if (holdTarget != null)
                NotifyHoldCanceled();

            current = newCurrent;
            ResetHold();
        }

        if (current != null && current.TryGetPrompt(this, out var prompt) && !string.IsNullOrEmpty(prompt))
            label = prompt;

        if (ui) ui.SetUnderCrosshairLabel(label);
    }


    private void HandleInteractInput()
    {
        if (current == null)
        {
            NotifyHoldCanceled();
            ResetHold();
            return;
        }

        float required = GetInteractDuration(current);

        if (required <= 0f)
        {
            SetRadial(0f, false);

            if (Input.GetKeyDown(interactKey))
            {
                if (current.Interact(this))
                {

                }
            }

            return;
        }

        if (Input.GetKey(interactKey))
        {
            if (holdTarget != current)
            {
                NotifyHoldCanceled();

                holdTarget = current;
                holdTimer = 0f;
                holdDuration = required;

                if (holdTarget is IHoldFeedback fb)
                    fb.OnHoldStart(this, holdDuration);
            }

            holdTimer += Time.deltaTime;
            float t = Mathf.Clamp01(holdTimer / Mathf.Max(holdDuration, 0.0001f));
            SetRadial(t, true);

            if (holdTimer >= holdDuration)
            {
                if (holdTarget != null && holdTarget.Interact(this))
                {

                }

                ResetHold();
            }
        }
        else
        {
            NotifyHoldCanceled();
            ResetHold();
        }
    }

    private void NotifyHoldCanceled()
    {
        if (holdTarget is IHoldFeedback fb)
            fb.OnHoldCanceled(this);
    }

    private float GetInteractDuration(IInteractable interactable)
    {
        if (interactable is IHoldInteractable hold)
        {
            float mult = PlayerStatManager.Instance != null
            ? PlayerStatManager.Instance.CurrentSnapshot.InteractionSpeedMultiplier
            : 1f;

            mult = Mathf.Max(mult, 0.05f);

            return hold.GetInteractDuration(this) / mult;
        }

        return 0f;
    }

    private void SetRadial(float value, bool visible)
    {
        if (ui == null || ui.radialProgressBar == null)
            return;

        ui.radialProgressBar.fillAmount = Mathf.Clamp01(value);
        ui.radialProgressBar.gameObject.SetActive(visible && value > 0f);
    }

    private void ResetHold()
    {
        holdTarget = null;
        holdTimer = 0f;
        holdDuration = 0f;
        SetRadial(0f, false);
    }

    private IInteractable RaycastForInteractable()
    {
        if (!cam) return null;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out var hit, useDistance, interactMask, QueryTriggerInteraction.Ignore))
            return hit.collider.GetComponentInParent<IInteractable>();

        return null;
    }

    public Inventory PlayerInventory => InventoryManager.Instance.playerInventory;
}
