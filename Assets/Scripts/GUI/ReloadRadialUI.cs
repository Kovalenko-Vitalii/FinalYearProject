using UnityEngine;
using UnityEngine.UI;

public class ReloadRadialUI : MonoBehaviour
{
    [SerializeField] private Image radialImage;
    [SerializeField] private GameObject root;

    private PlayerItemController currentPlayer;
    private HeldFirearmItem currentFirearm;

    private void Awake()
    {
        SetVisible(false);
        SetProgress(0f);
    }

    private void OnEnable()
    {
        TryRebindAll();
    }

    private void OnDisable()
    {
        UnbindFirearm();
        currentPlayer = null;
    }

    private void Update()
    {
        TryRebindAll();
    }

    private void TryRebindAll()
    {
        PlayerItemController foundPlayer = FindFirstObjectByType<PlayerItemController>();

        if (currentPlayer != foundPlayer)
        {
            UnbindFirearm();
            currentPlayer = foundPlayer;
        }

        HeldFirearmItem foundFirearm = null;
        if (currentPlayer != null)
            foundFirearm = currentPlayer.CurrentHeldItem as HeldFirearmItem;

        if (currentFirearm != foundFirearm)
        {
            UnbindFirearm();
            currentFirearm = foundFirearm;
            BindFirearm();
        }

        if (currentPlayer == null)
        {
            SetVisible(false);
            SetProgress(0f);
        }
    }

    private void BindFirearm()
    {
        if (currentFirearm == null)
        {
            SetVisible(false);
            SetProgress(0f);
            return;
        }

        currentFirearm.OnReloadProgressChanged += HandleReloadProgressChanged;
        currentFirearm.OnReloadStateChanged += HandleReloadStateChanged;

        var state = currentFirearm.ItemInstance?.firearmState;
        if (state != null)
        {
            SetVisible(state.isReloading);
            SetProgress(state.reloadProgress01);
        }
        else
        {
            SetVisible(false);
            SetProgress(0f);
        }
    }

    private void UnbindFirearm()
    {
        if (currentFirearm != null)
        {
            currentFirearm.OnReloadProgressChanged -= HandleReloadProgressChanged;
            currentFirearm.OnReloadStateChanged -= HandleReloadStateChanged;
            currentFirearm = null;
        }
    }

    private void HandleReloadProgressChanged(float progress)
    {
        SetProgress(progress);
    }

    private void HandleReloadStateChanged(bool isReloading)
    {
        SetVisible(isReloading);

        if (!isReloading)
            SetProgress(0f);
    }

    private void SetProgress(float value)
    {
        if (radialImage != null)
            radialImage.fillAmount = Mathf.Clamp01(value);
    }

    private void SetVisible(bool visible)
    {
        if (root != null)
            root.SetActive(visible);
        else if (radialImage != null)
            radialImage.gameObject.SetActive(visible);
    }
}