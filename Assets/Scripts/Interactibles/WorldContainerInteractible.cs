using UnityEngine;

[RequireComponent(typeof(WorldContainer))]
public class WorldContainerInteractable : MonoBehaviour, IInteractable, IHoldInteractable, IHoldFeedback
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
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip openSound;
    [SerializeField] AudioClip closeSound;

    private bool isOpen = false;
    private bool isSearched = false;

    private WorldContainer container;

    private void Awake()
    {
        container = GetComponent<WorldContainer>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void SetOpen(bool value)
    {
        isOpen = value;

        if (animator != null && !string.IsNullOrEmpty(isOpenBoolName))
            animator.SetBool(isOpenBoolName, isOpen);
    }

    public bool TryGetPrompt(PlayerInteractor interactor, out string prompt)
    {
        prompt = $"Open {displayName}";
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
        SetOpen(true);
    }

    public void OnHoldCanceled(PlayerInteractor interactor)
    {
        if (!CanvasSwitcher.Instance.AnyOpen)
            SetOpen(false);
    }

    public void CloseLid()
    {
        PlayCloseSound();
        SetOpen(false);
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
}
