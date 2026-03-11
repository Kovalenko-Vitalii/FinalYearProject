using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float backwardSpeed = 3f;

    [Tooltip("How fast we accelerate to target speed")]
    [SerializeField] private float speedUpSmoothness = 12f;

    [Tooltip("How fast we decelerate to target speed")]
    [SerializeField] private float speedDownSmoothness = 16f;

    [Header("Stamina Sprint")]
    [SerializeField] private float sprintStaminaDrainPerSecond = 14f;
    [SerializeField] private float sprintRegenDelayAfterUse = 0.75f;


    [Header("Gravity")]
    [SerializeField] private float gravityForce = 25f;
    [SerializeField] private float groundedStickForce = 2f;

    [Header("Slope Handling")]
    [SerializeField] private float slideForce = 5f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask whatIsGround;

    [Header("Links")]
    [SerializeField] private Transform orientation;
    [SerializeField] private CharacterController controller;

    public float VerticalVelocity => _velocity.y;
    public bool IsGrounded => controller.isGrounded;
    public enum MovementState { Walking, Sprinting, Air }
    public MovementState State { get; private set; }

    public Vector3 MoveDirection => _moveDirection;
    public static bool canMove = true;

    private float _currentSpeed;
    private float _horizontalInput;
    private float _verticalInput;

    private Vector3 _moveDirection;
    private Vector3 _velocity;

    private StatusEffectsSnapshot _effectsSnapshot;

    public bool IsSprinting => State == MovementState.Sprinting;


    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (orientation == null)
        {
            var cam = Camera.main;
            if (cam != null) orientation = cam.transform;
        }

        _currentSpeed = walkSpeed;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        CacheStatusEffects();

        ReadInput();
        HandleState();
        ApplyGravity();
        Move();
    }

    private void CacheStatusEffects()
    {
        var stats = PlayerStatManager.Instance;
        _effectsSnapshot = (stats != null) ? stats.CurrentSnapshot : StatusEffectsSnapshot.Default;
    }


    private void ReadInput()
    {
        if (!canMove)
        {
            _horizontalInput = 0f;
            _verticalInput = 0f;
            return;
        }

        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void HandleState()
    {
        bool grounded = controller.isGrounded;
        var stats = PlayerStatManager.Instance;

        bool sprintKey = grounded
                         && _effectsSnapshot.CanSprint
                         && Input.GetKey(KeyCode.LeftShift)
                         && _verticalInput > 0f;

        bool isSprintingNow = (State == MovementState.Sprinting);

        bool allowedByStamina =
            stats == null
            || (isSprintingNow ? stats.CanKeepSprinting() : stats.CanStartSprint());

        bool wantsSprint = sprintKey && allowedByStamina;

        if (wantsSprint && stats != null)
        {
            float drain = sprintStaminaDrainPerSecond
                          * _effectsSnapshot.StaminaDrainMultiplier
                          * Time.deltaTime;

            float actualDrain = Mathf.Min(drain, stats.Stamina);
            stats.TryConsumeStamina(actualDrain, sprintRegenDelayAfterUse);
        }

        float baseSpeed;
        float smooth;

        if (!grounded)
        {
            State = MovementState.Air;
            baseSpeed = walkSpeed;
            smooth = speedDownSmoothness;
        }
        else if (wantsSprint)
        {
            State = MovementState.Sprinting;
            baseSpeed = sprintSpeed;
            smooth = speedUpSmoothness;
        }
        else
        {
            State = MovementState.Walking;
            baseSpeed = (_verticalInput < 0f) ? backwardSpeed : walkSpeed;
            smooth = speedDownSmoothness;
        }

        float targetSpeed = baseSpeed * _effectsSnapshot.MoveSpeedMultiplier;
        _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, Time.deltaTime * smooth);
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded)
        {
            // keep player grounded
            if (_velocity.y < 0f)
                _velocity.y = -groundedStickForce;
        }
        else
        {
            _velocity.y -= gravityForce * Time.deltaTime;
        }
    }

    private void Move()
    {
        if (!canMove)
        {
            // still apply gravity so you don't freeze mid-air if canMove toggles
            controller.Move(_velocity * Time.deltaTime);
            return;
        }

        Vector3 forward = orientation ? orientation.forward : transform.forward;
        Vector3 right = orientation ? orientation.right : transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 inputDir = (forward * _verticalInput + right * _horizontalInput);
        if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();

        _moveDirection = inputDir * _currentSpeed;

        // rotate player to camera yaw
        if (orientation)
            transform.rotation = Quaternion.Euler(0f, orientation.eulerAngles.y, 0f);

        // slope slide if too steep
        if (TryGetSteepSlopeSlide(out Vector3 slideDir))
        {
            _moveDirection += slideDir * slideForce;
        }

        Vector3 finalMove = _moveDirection;
        finalMove.y = _velocity.y;

        controller.Move(finalMove * Time.deltaTime);
    }

    private bool TryGetSteepSlopeSlide(out Vector3 slideDirection)
    {
        slideDirection = Vector3.zero;

        // only relevant when grounded-ish
        if (!controller.isGrounded) return false;

        // spherecast from center down to detect normal
        Vector3 origin = transform.position + Vector3.up * (controller.height * 0.5f);
        float radius = controller.radius * 0.95f;

        if (Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit hit, controller.height * 0.6f, whatIsGround, QueryTriggerInteraction.Ignore))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            if (slopeAngle > controller.slopeLimit)
            {
                // slide down along the slope
                slideDirection = Vector3.ProjectOnPlane(Vector3.down, hit.normal).normalized;
                return true;
            }
        }

        return false;
    }
}
