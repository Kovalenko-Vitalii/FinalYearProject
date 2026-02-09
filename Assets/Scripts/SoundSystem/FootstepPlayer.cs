using System;
using UnityEngine;

public class FootstepPlayer : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private PlayerMovement movement;

    [Header("Clips per surface")]
    [SerializeField] private SurfaceClips[] surfaceClips;

    [Serializable]
    public class SurfaceClips
    {
        public SurfaceType type = SurfaceType.Default;
        public AudioClip[] clips;
    }

    private void Awake()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (movement == null) movement = GetComponent<PlayerMovement>();


    }
}
