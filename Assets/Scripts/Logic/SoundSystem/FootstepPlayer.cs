using System;
using UnityEngine;

public class FootstepPlayer : MonoBehaviour, IPlayerTick
{
    [Header("Links")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private PlayerMovement movement;

    [Header("Raycast")]
    [SerializeField] LayerMask groundMask = ~0;

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

        Vector3 vel = controller.velocity; 
        vel.y = 0f;
        bool moving = vel.magnitude > moveSpeedThreshold;

        if (!controller.isGrounded || !moving)
        {
            timer = 0f;
            return;
        }

        float interval = (movement != null && movement.IsSprinting) ? sprintInterval : walkInterval;

        timer += dt;
        if (timer < interval) return;
        timer = 0f;

        SurfaceType surface = GetSurface();
        AudioClip clip = GetClipForSurface(surface);
        if (!clip) return;

        lastClip = clip;

        var volumeMul = 1 + PlayerStatManager.Instance.CurrentWeight / 100;
        var pitch = 1 - PlayerStatManager.Instance.CurrentWeight / 100;
        SoundManager.Instance.PlayFootstep(clip, volumeMul, pitch);
    }

    // This method gets random clip associated with provided surface 
    AudioClip GetClipForSurface(SurfaceType surface)
    {
        AudioClip[] selectedClips = null;

        foreach (var clip in surfaceClips)
            if (clip.type == surface)
                selectedClips = clip.clips;
        if (selectedClips.Length > 0)
            return selectedClips[UnityEngine.Random.Range(0, selectedClips.Length)];
        else
            return null;
    }

    // This method gets surface type depending on what is under player
    SurfaceType GetSurface()
    {
        Vector3 origin = transform.position + Vector3.down * (controller.height * 0.5f);
        float rayLength = (controller.height * 0.5f) + 0.6f;
        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayLength, groundMask, QueryTriggerInteraction.Ignore))    
            return SurfaceType.Default;
            
        // Priority for model
        SurfaceTypeHolder surfaceOnModel = hit.collider.GetComponent<SurfaceTypeHolder>();
        if (surfaceOnModel)
            return surfaceOnModel.type;
            

        if(hit.collider is TerrainCollider)
        {
            Terrain terrain = hit.collider.GetComponent<Terrain>() ?? Terrain.activeTerrain;            
            if (terrain)
            {
                // Getting terrain data
                TerrainData data = terrain.terrainData;

                // Returning if there is no layers
                if (data.terrainLayers == null || data.terrainLayers.Length == 0) return SurfaceType.Default;

                // Position on map
                Vector3 pos = hit.point - terrain.transform.position;

                // Finding points x and z on terrain
                int x = Mathf.RoundToInt((pos.x / data.size.x) * (data.alphamapWidth - 1));
                int z = Mathf.RoundToInt((pos.z / data.size.z) * (data.alphamapHeight - 1));

                // Getting array of layers on our coordinates
                float[,,] alpha = data.GetAlphamaps(x, z, 1, 1);

                int best = 0;
                float bestVal = 0f;
                int layers = alpha.GetLength(2);

                for (int i = 0; i < layers; i++)
                {
                    float v = alpha[0, 0, i];
                    if (v > bestVal) { bestVal = v; best = i; }
                }

                TerrainLayer layer = data.terrainLayers[best];

                // Finding one we need
                for (int i = 0; i < terrainLayerSurfaces.Length; i++)
                    if (terrainLayerSurfaces[i].layer == layer)
                        return terrainLayerSurfaces[i].type;
            }
        }
        // If so return default
        return SurfaceType.Default;
    }
}
