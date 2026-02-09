using System;
using UnityEngine;

public class FootstepPlayer : MonoBehaviour, IPlayerTick
{
    [Header("Links")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private PlayerMovement movement;

    [Header("Step timing")]
    [SerializeField] float walkInterval = 0.45f;
    [SerializeField] float sprintInterval = 0.30f;
    [SerializeField] float moveSpeedThreshold = 0.2f;

    [Header("Terrain layer to surface")]
    [SerializeField] private TerrainLayerSurface[] terrainLayerSurfaces;

    [Header("Clips per surface")]
    [SerializeField] private SurfaceClips[] surfaceClips;

    float timer;
    AudioClip lastClip;

    [Serializable]
    public class SurfaceClips
    {
        public SurfaceType type = SurfaceType.Default;
        public AudioClip[] clips;
    }

    [Serializable]
    public class TerrainLayerSurface
    {
        public TerrainLayer layer;
        public SurfaceType type;
    }

    // ======================================================================================

    void OnEnable() => PlayerTickSystem.Instance?.Register(this);
    void OnDisable() => PlayerTickSystem.Instance?.Unregister(this);

    private void Awake()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (movement == null) movement = GetComponent<PlayerMovement>();
    }

    // ======================================================================================
    public void Tick(float dt)
    {
        if (SoundManager.Instance == null || controller == null) return;


    }

    SurfaceType GetSurface()
    {
        Vector3 origin = transform.position + Vector3.up * (controller.height * 0.5f);
        float rayLength = (controller.height * 0.5f) + 0.6f;
        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayLength))
            return SurfaceType.Default;

        // Priority for model
        SurfaceSound surfaceOnModel = hit.collider.GetComponent<SurfaceSound>();
        if (surfaceOnModel)
            return surfaceOnModel.type;

        if(hit.collider is TerrainCollider)
        {
            Terrain terrain = hit.collider.GetComponent<Terrain>() ?? Terrain.activeTerrain;            
            if (terrain)
            {
                TerrainData data = terrain.terrainData;
                Vector3 pos = hit.point - terrain.transform.position;

                int x = Mathf.RoundToInt((pos.x / data.size.x) * (data.alphamapWidth - 1));
            }
        }
        
        return SurfaceType.Default;
    }
}
