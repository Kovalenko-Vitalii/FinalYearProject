using System;
using UnityEngine;

public class ObstacleInteractible : MonoBehaviour, IInteractable, IHoldInteractable, IHoldFeedback
{
    [SerializeField] requiredItem[] requiredItems;
    [SerializeField] float duration;
    [SerializeField] float hungerCost;
    [SerializeField] float hydrationCost;
    [SerializeField] float energyCost;
    [SerializeField] float timeCost;

    [Serializable]
    public class requiredItem
    {
        public ItemData data;
        public int amount = 1;
        public bool delete;
        float durabilityCost;
    }

    public float GetInteractDuration(PlayerInteractor interactor)
    {
        return duration;
    }

    public bool Interact(PlayerInteractor interactor)
    {
        throw new NotImplementedException();
    }

    public void OnHoldCanceled(PlayerInteractor interactor) { }

    public void OnHoldStart(PlayerInteractor interactor, float duration)
    {
        throw new NotImplementedException();
    }

    public bool TryGetPrompt(PlayerInteractor interactor, out string prompt)
    {
        string list = "";
        foreach (var item in requiredItems)
            list += item.data.itemName + " " + "x(" + item.amount + ")";

        prompt = "To break this obstacle you need: " + list;
        return true;
    }
}
