using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float speedUpSmoothness;
    public float speedDownSmoothness;
    public float backwardSpeed;

    public float groundDrag;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    public float gravityForce;

    [Header("===Links===")]
    public Transform orientation;
    public CharacterController controller;

    [Header("===MOVING STATE===")]
    public MovementState state;

    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;
    Vector3 playerVelocity;
    public static bool canMove;

    [Header("Slope Handling")]
    public float maxSlopeAngle = 45f;
    public float slideForce = 5f;

    public Vector3 MoveDirection => moveDirection;
    public MovementState CurrentState => state;

    private bool OnSlope(out Vector3 slopeDirection, out float slopeAngle)
    {
        slopeDirection = Vector3.zero;
        slopeAngle = 0f;

        if (Physics.SphereCast(transform.position, controller.radius, Vector3.down, out RaycastHit hit, playerHeight / 2 + 1f, whatIsGround))
        {
            slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            if (slopeAngle > controller.slopeLimit)
            {
                slopeDirection = Vector3.ProjectOnPlane(Vector3.down, hit.normal).normalized;
                return true;
            }
        }
        return false;
    }

    public enum MovementState
    {
        walking,
        sprinting,
        air
    }

    private void Start()
    {
        controller = transform.GetComponent<CharacterController>();
        playerHeight = controller.height;

        canMove = true;
        moveSpeed = walkSpeed;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        grounded = controller.isGrounded;

        StateHandler();
        MovePlayer();
    }

    private void StateHandler()
    {
        // Mode - Sprinting
        if (grounded && Input.GetKey(KeyCode.LeftShift) && verticalInput > 0)
        {
            state = MovementState.sprinting;
            moveSpeed = Mathf.Lerp(moveSpeed, sprintSpeed, Time.deltaTime * speedUpSmoothness);
        }
        // Mode - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = verticalInput < 0 ? backwardSpeed : walkSpeed;
        }
        // Mode - Air
        else
        {
            state = MovementState.air;
        }
    }

    private void MovePlayer()
    {
        if (grounded)
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            verticalInput = Input.GetAxisRaw("Vertical");
        }

        Vector3 cameraForward = orientation.forward;
        cameraForward.y = 0;

        moveDirection = cameraForward.normalized * verticalInput + orientation.right * horizontalInput;

        moveDirection *= moveSpeed;

        transform.rotation = Quaternion.Euler(0, orientation.rotation.eulerAngles.y, 0);

        if (!controller.isGrounded)
        {
            playerVelocity.y -= gravityForce * Time.deltaTime;
        }

        moveDirection += playerVelocity;

        if (!controller.isGrounded)
        {
            controller.Move(moveDirection * Time.deltaTime);
            return;
        }

        if (OnSlope(out Vector3 slopeDir, out float slopeAngle))
        {
            controller.Move(slopeDir * slideForce * Time.deltaTime);
        }
        else
        {
            controller.Move(moveDirection * Time.deltaTime);
        }
    }
}
