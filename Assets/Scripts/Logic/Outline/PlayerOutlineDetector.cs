using System.Collections.Generic;
using UnityEngine;

// This class is responsible for detecting objects with outline around player and turning it on if in radius
public class PlayerInteractableRadiusHighlighter : MonoBehaviour
{
    [SerializeField] private Transform origin;

    [Header("Radius")]
    [SerializeField] private float enterRadius = 3.5f;
    [SerializeField] private float exitRadius = 4.0f;

    [Header("Scan")]
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private float scanInterval = 0.1f;
    [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;

    private readonly Collider[] hits = new Collider[256];
    private readonly Dictionary<MonoBehaviour, float> nearestDistance = new();
    private readonly HashSet<MonoBehaviour> active = new();

    private float nextScanTime;

    private void Reset()
    {
        origin = transform;
    }

    private void Update()
    {
        if (Time.time < nextScanTime)
            return;

        nextScanTime = Time.time + scanInterval;
        Scan();
    }

    private void Scan()
    {
        nearestDistance.Clear();

        int count = Physics.OverlapSphereNonAlloc(
            origin.position,
            exitRadius,
            hits,
            interactableMask,
            triggerInteraction);

        for (int i = 0; i < count; i++)
        {
            Collider col = hits[i];
            if (col == null)
                continue;

            MonoBehaviour target = GetHighlightTarget(col);
            if (target == null)
                continue;

            Vector3 closest = col.ClosestPoint(origin.position);
            float sqrDistance = (closest - origin.position).sqrMagnitude;

            if (nearestDistance.TryGetValue(target, out float oldSqr))
            {
                if (sqrDistance < oldSqr)
                    nearestDistance[target] = sqrDistance;
            }
            else
            {
                nearestDistance.Add(target, sqrDistance);
            }
        }

        foreach (var pair in nearestDistance)
        {
            MonoBehaviour target = pair.Key;
            float dist = Mathf.Sqrt(pair.Value);

            bool isActive = active.Contains(target);

            if (!isActive && dist <= enterRadius)
            {
                SetHighlight(target, true);
                active.Add(target);
            }
            else if (isActive && dist <= exitRadius)
            {
                SetHighlight(target, true);
            }
        }

        active.RemoveWhere(target =>
        {
            if (target == null)
                return true;

            if (!nearestDistance.TryGetValue(target, out float sqrDist))
            {
                SetHighlight(target, false);
                return true;
            }

            float dist = Mathf.Sqrt(sqrDist);
            if (dist > exitRadius)
            {
                SetHighlight(target, false);
                return true;
            }

            return false;
        });

        if (count == hits.Length)
        {
            Debug.LogWarning("PlayerInteractableRadiusHighlighter: buffer full, increase hits array size.");
        }
    }

    private MonoBehaviour GetHighlightTarget(Collider col)
    {
        var obstacle = col.GetComponentInParent<ObstacleOutline>();
        if (obstacle != null)
            return obstacle;

        var outline = col.GetComponentInParent<InteractableOutline>();
        if (outline != null)
            return outline;

        return null;
    }

    private void SetHighlight(MonoBehaviour target, bool visible)
    {
        switch (target)
        {
            case ObstacleOutline obstacle:
                if (visible) obstacle.Show();
                else obstacle.Hide();
                break;

            case InteractableOutline outline:
                if (visible) outline.Show();
                else outline.Hide();
                break;
        }
    }

    private void OnDisable()
    {
        foreach (var target in active)
        {
            if (target != null)
                SetHighlight(target, false);
        }

        active.Clear();
        nearestDistance.Clear();
    }
}