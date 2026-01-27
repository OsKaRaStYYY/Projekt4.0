using UnityEngine;
using UnityEngine.InputSystem; // Wymagane dla nowego systemu

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class FPS_Movement : MonoBehaviour
{
    [Header("Referencje")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private CapsuleCollider playerCollider;

    [Header("Input System - Akcje")]
    [SerializeField] private InputActionProperty moveAction;
    [SerializeField] private InputActionProperty lookAction;
    [SerializeField] private InputActionProperty jumpAction;
    [SerializeField] private InputActionProperty sprintAction;
    [SerializeField] private InputActionProperty crouchAction;

    [Header("Ruch")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8.5f;
    [SerializeField] private float crouchSpeed = 2f;
    [SerializeField] private float groundAcceleration = 20f;
    [SerializeField] private float airAcceleration = 15f;
    [SerializeField] private float airSpeedCap = 1.2f;

    [Header("Skoki")]
    [SerializeField] private float jumpForce = 6.5f;
    [SerializeField] private float jumpCooldown = 0.8f;

    [Header("Fizyka")]
    [SerializeField] private float groundDrag = 6f;
    [SerializeField] private float airDrag = 1f;
    [SerializeField] private LayerMask groundMask;

    [Header("Kucanie")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchingHeight = 1f;
    [SerializeField] private float crouchSmoothTime = 0.2f;
    [SerializeField] private float cameraCrouchOffset = 0.5f;

    [Header("Kamera")]
    [SerializeField] private float sensitivityX = 0.1f; // Nowy system ma inne wartoœci delta
    [SerializeField] private float sensitivityY = 0.1f;
    [SerializeField] private float maxCameraAngle = 90f;

    [Header("Grawitacja")]
    [SerializeField] private float baseGravity = 9.81f;
    [SerializeField] private float fallMultiplier = 1.15f;
    [SerializeField] private float lowJumpMultiplier = 1f;

    private Rigidbody rb;
    private float horizontalInput;
    private float verticalInput;
    private bool grounded;
    private bool readyToJump = true;
    private float currentHeight;
    private float heightVelocity;
    private Vector3 cameraPosVelocity;
    private Vector3 initialCameraPosition;
    private float rotationX;

    void OnEnable()
    {
        moveAction.action.Enable();
        lookAction.action.Enable();
        jumpAction.action.Enable();
        sprintAction.action.Enable();
        crouchAction.action.Enable();
    }

    void OnDisable()
    {
        moveAction.action.Disable();
        lookAction.action.Disable();
        jumpAction.action.Disable();
        sprintAction.action.Disable();
        crouchAction.action.Disable();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        currentHeight = standingHeight;
        initialCameraPosition = playerCamera.localPosition;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        RotateCamera();
        GroundCheck();
        HandleInput();
        ControlSpeed();
        ControlDrag();
        HandleCrouch();
        ApplyCustomGravity();
    }

    private void ApplyCustomGravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.AddForce(Vector3.down * baseGravity * (fallMultiplier - 1) * rb.mass);
        }
        else if (rb.linearVelocity.y > 0 && !jumpAction.action.IsPressed())
        {
            rb.AddForce(Vector3.down * baseGravity * (lowJumpMultiplier - 1) * rb.mass);
        }
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    private void GroundCheck()
    {
        float sphereRadius = playerCollider.radius * 0.9f;
        Vector3 spherePosition = transform.position + Vector3.up * (sphereRadius + 0.1f);
        grounded = Physics.SphereCast(spherePosition, sphereRadius, Vector3.down, out _, 0.2f, groundMask);
    }

    private void HandleInput()
    {
        Vector2 input = moveAction.action.ReadValue<Vector2>();
        horizontalInput = input.x;
        verticalInput = input.y;

        if (jumpAction.action.triggered && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MovePlayer()
    {
        Vector3 moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        bool isSprinting = sprintAction.action.IsPressed() && !crouchAction.action.IsPressed() && verticalInput > 0;
        float currentSpeed = crouchAction.action.IsPressed() ? crouchSpeed : (isSprinting ? sprintSpeed : walkSpeed);

        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * currentSpeed * groundAcceleration, ForceMode.Acceleration);
        }
        else
        {
            float speedMultiplier = Mathf.Clamp01(1 - Vector3.Dot(rb.linearVelocity.normalized, moveDirection.normalized));
            float currentAirSpeedCap = currentSpeed * airSpeedCap;

            if (new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude < currentAirSpeedCap)
            {
                rb.AddForce(moveDirection.normalized * airAcceleration * speedMultiplier, ForceMode.Acceleration);
            }
        }
    }

    private void ControlSpeed()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        bool isSprinting = sprintAction.action.IsPressed() && !crouchAction.action.IsPressed() && verticalInput > 0;
        float currentSpeed = crouchAction.action.IsPressed() ? crouchSpeed : (isSprinting ? sprintSpeed : walkSpeed);

        if (flatVel.magnitude > currentSpeed && grounded)
        {
            Vector3 limitedVel = flatVel.normalized * currentSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void ControlDrag()
    {
        rb.linearDamping = grounded ? groundDrag : airDrag;
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void RotateCamera()
    {
        Vector2 mouseDelta = lookAction.action.ReadValue<Vector2>();

        float mouseX = mouseDelta.x * sensitivityX;
        float mouseY = mouseDelta.y * sensitivityY;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -maxCameraAngle, maxCameraAngle);

        playerCamera.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleCrouch()
    {
        bool isCrouchingRequested = crouchAction.action.IsPressed();

        if (isCrouchingRequested)
        {
            currentHeight = Mathf.SmoothDamp(currentHeight, crouchingHeight, ref heightVelocity, crouchSmoothTime);
        }
        else
        {
            if (Physics.SphereCast(transform.position, playerCollider.radius, Vector3.up, out _, standingHeight - crouchingHeight))
            {
                return;
            }
            currentHeight = Mathf.SmoothDamp(currentHeight, standingHeight, ref heightVelocity, crouchSmoothTime);
        }

        playerCollider.height = currentHeight;
        playerCollider.center = Vector3.up * (currentHeight / 2);

        float heightRatio = currentHeight / standingHeight;
        Vector3 targetCameraPos = initialCameraPosition - new Vector3(0, cameraCrouchOffset * (1 - heightRatio), 0);
        playerCamera.localPosition = Vector3.SmoothDamp(
            playerCamera.localPosition,
            targetCameraPos,
            ref cameraPosVelocity,
            crouchSmoothTime
        );
    }
}
