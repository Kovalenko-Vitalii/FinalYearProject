using UnityEngine;

// This class creates additional layer of outline management script for interactible objects
[RequireComponent(typeof(Outline))]
public class InteractableOutline : MonoBehaviour
{
    [SerializeField] private Outline outline;
    [SerializeField] private Color defaultColor = Color.white;

    private void Reset()
    {
        outline = GetComponent<Outline>();
    }

    private void Awake()
    {
        if (outline == null)
            outline = GetComponent<Outline>();

        outline.enabled = false;
    }

    public void Show()
    {
        outline.OutlineColor = defaultColor;
        outline.OutlineWidth = 8f;
        outline.OutlineMode = Outline.Mode.OutlineAndSilhouette;
        outline.enabled = true;
    }

    public void Show(Color color)
    {
        outline.OutlineColor = color;
        outline.OutlineWidth = 8f;
        outline.OutlineMode = Outline.Mode.OutlineAndSilhouette;
        outline.enabled = true;
    }

    public void Hide()
    {
        outline.enabled = false;
    }
}