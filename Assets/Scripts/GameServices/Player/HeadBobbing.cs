using UnityEngine;

// I have moved it from my old game and modified so it runs in current project
public class HeadBobbing : MonoBehaviour, IPlayerTick
{
    [Header("Links")]
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private CharacterController controller;
    [SerializeField] private PlayerMovement movement;

    [Header("Bobbing")]
    [SerializeField] private float walkFrequency = 1.6f;
    [SerializeField] private float sprintFrequency = 2.8f;

    [SerializeField] private float walkHeight = 0.045f;
    [SerializeField] private float sprintHeight = 0.095f;

    [SerializeField] private float smoothReturnSpeed = 10f;

    [Header("When to bob")]
    [SerializeField] private float moveSpeedThreshold = 0.2f;

    private Vector3 initialLocalPos;
    private float timer;

    private void Awake()
    {
        if (controller == null) controller = GetComponentInParent<CharacterController>();
        if (movement == null) movement = GetComponentInParent<PlayerMovement>();
        if (cameraHolder == null) cameraHolder = transform;
    }

    void OnEnable() => PlayerTickSystem.Instance?.Register(this);
    void OnDisable() => PlayerTickSystem.Instance?.Unregister(this);

    private void Start() => initialLocalPos = cameraHolder.localPosition;
       
    public void Tick(float dt)
    {
        if (cameraHolder == null || controller == null) return;

        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
        {
            ReturnToCenter();
            return;
        }

        Vector3 vel = controller.velocity;
        vel.y = 0f;

        bool grounded = controller.isGrounded;
        bool moving = vel.magnitude > moveSpeedThreshold;

        if (!grounded || !moving)
        {
            ReturnToCenter();
            return;
        }

        bool sprint = (movement != null && movement.IsSprinting);

        float freq = sprint ? sprintFrequency : walkFrequency;
        float height = sprint ? sprintHeight : walkHeight;

        float speed01 = Mathf.InverseLerp(0.0f, sprint ? 7f : 4f, vel.magnitude);
        float freqMul = Mathf.Lerp(0.9f, 1.15f, speed01);
        float heightMul = Mathf.Lerp(0.85f, 1.15f, speed01);

        timer += Time.deltaTime * freq * freqMul;

        float y = Mathf.Sin(timer) * height * heightMul;
        float x = Mathf.Cos(timer * 0.5f) * (height * 0.35f) * heightMul;

        cameraHolder.localPosition = initialLocalPos + new Vector3(x, y, 0f);
    }

    private void ReturnToCenter()
    {
        cameraHolder.localPosition = Vector3.Lerp(cameraHolder.localPosition, initialLocalPos, Time.deltaTime * smoothReturnSpeed);
        timer = 0f;
    }
}
