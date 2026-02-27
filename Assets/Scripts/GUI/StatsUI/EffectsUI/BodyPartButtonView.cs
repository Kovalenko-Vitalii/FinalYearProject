using System;
using UnityEngine;
using UnityEngine.UI;

public class BodyPartButtonView : MonoBehaviour
{
    [SerializeField] private BodyPart part;
    [SerializeField] private Button button;
    [SerializeField] private GameObject root;

    public BodyPart Part => part;

    public event Action<BodyPart> Clicked;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(() => Clicked?.Invoke(part));
    }

    public void SetHasEffects(bool hasEffects)
    {
        if (root == null) root = gameObject;

        root.SetActive(hasEffects);
    }

}
