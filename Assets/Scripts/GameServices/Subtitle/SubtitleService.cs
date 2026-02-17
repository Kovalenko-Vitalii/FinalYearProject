using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubtitleService : MonoBehaviour
{
    public static SubtitleService Instance { get; private set; }

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
                SoundManager.Instance.PlaySubtitle(clip);
                duration = Mathf.Max(0.05f, clip.length);
            }

            yield return new WaitForSeconds(duration);

            view.Hide();
        }

        isPlaying = false;
        routine = null;
    }

    /*
    private IEnumerator PlayLoop()
    {
        isPlaying = true;

        while (queue.Count > 0)
        {
            var req = queue.Dequeue();
            if (req?.line == null) continue;

            view.Show(req);

            float duration = req.line.duration;

            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.clip = req.line.voice;

                if (audioSource.clip != null)
                {
                    audioSource.Play();
                    duration = Mathf.Max(0.05f, audioSource.clip.length);
                }
            }

            yield return new WaitForSeconds(duration);

            view.Hide();
            if (audioSource != null) audioSource.Stop();
        }

        isPlaying = false;
        routine = null;
    }
    */

}
