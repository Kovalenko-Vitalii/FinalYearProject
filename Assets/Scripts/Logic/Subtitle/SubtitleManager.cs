using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This manager is responsible for managing subtitle queue
public class SubtitleManager : MonoBehaviour
{
    public static SubtitleManager Instance { get; private set; }

    [SerializeField] private SubtitleView view;

    private readonly Queue<LineRequest> queue = new();
    private Coroutine routine;
    private bool isPlaying;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Play(LineData line)
    {
        if (line == null) return;

        queue.Enqueue(new LineRequest(line));

        if (!isPlaying)
            routine = StartCoroutine(PlayLoop());
    }

    public void PlaySequence(IEnumerable<LineData> lines)
    {
        if (lines == null) return;

        foreach (var line in lines)
        {
            if (line == null) continue;
            queue.Enqueue(new LineRequest(line));
        }

        if (!isPlaying)
            routine = StartCoroutine(PlayLoop());
    }

    private IEnumerator PlayLoop()
    {
        isPlaying = true;

        while (queue.Count > 0)
        {
            var req = queue.Dequeue();
            if (req?.line == null) continue;

            view.Show(req);

            float duration = req.line.duration;

            var clip = req.line.voice;

            if (clip)
            {
                SoundManager.Instance.PlaySubtitleSound(clip);
                duration = Mathf.Max(0.05f, clip.length);
            }

            yield return new WaitForSeconds(duration);

            view.Hide();
        }

        isPlaying = false;
        routine = null;
    }
}

// This class is just sketch for future
// Simply it is (when and according to which rules to say)
public sealed class LineRequest
{
    public LineData line;
    public LineRequest(LineData line) => this.line = line;
}
