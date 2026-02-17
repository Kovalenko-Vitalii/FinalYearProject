using UnityEngine;

// This script should be attached to collider and will trigger subtitle replica
[RequireComponent(typeof(Collider))]
public class TriggerLine : MonoBehaviour
{
    [SerializeField] private LineData line;
    [SerializeField] private bool playOnlyOnce = true;

    private bool played;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playOnlyOnce && played) return;

        SubtitleService.Instance?.Play(line);
        played = true;
    }
}
