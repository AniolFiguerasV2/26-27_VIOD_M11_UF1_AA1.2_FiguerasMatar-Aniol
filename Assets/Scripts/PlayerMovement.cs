using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerMovement : MonoBehaviour
{
    public float movementSpeed = 5f;
    public float jumpForce = 5f;
    public float gravity = -9f;

    public Transform pivotPoint;
    public float mouseSensivity = 0.2f;
    public float minCamLimit = -80f;
    public float maxCamLimit = 80f;

    private PlayerInputs inputAction;
    private CharacterController characterController;

    public float groundCheckDistance = 0.2f;
    public string groundTag = "Floor";

    private Vector2 look;
    private Vector2 movement;

    private float rotationY;
    private Vector3 vel;
    private bool isGrounded;
    private bool isSprinting;

    private void Awake()
    {
        inputAction = new PlayerInputs();
        characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        inputAction.Player.Enable();

        inputAction.Player.Move.performed += SetMovement;
        inputAction.Player.Move.canceled += player => movement = Vector2.zero;

        inputAction.Player.Look.performed += SetLook;
        inputAction.Player.Look.canceled += obj => look = Vector2.zero;

        inputAction.Player.Jump.performed += SetJump;

        inputAction.Player.Sprinting.performed += player => isSprinting = true;
        inputAction.Player.Sprinting.canceled += player => isSprinting = false;
    }
    private void Update()
    {
        GroundCheck();
        Look();
        Movement();
    }
    
    private void SetLook(InputAction.CallbackContext obj)
    {
        look = obj.ReadValue<Vector2>();
    }
    private void SetMovement(InputAction.CallbackContext obj)
    {
        movement = obj.ReadValue<Vector2>(); 
    }
    private void SetJump(InputAction.CallbackContext obj)
    {
        if (isGrounded)
        {
            vel.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }


    private void GroundCheck()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, characterController.height / 2 + groundCheckDistance))
        {
            isGrounded = hit.collider.CompareTag(groundTag);
        }
        else
        {
            isGrounded = false;
        }

        if (isGrounded && vel.y < 0)
        {
            vel.y = -2f;
        }
    }
    private void Look()
    {
        Vector2 mouseNormalized = look * mouseSensivity;

        rotationY = Mathf.Clamp(rotationY - mouseNormalized.y, minCamLimit, maxCamLimit);

        pivotPoint.localRotation = Quaternion.Euler(rotationY, 0, 0);
        transform.Rotate(Vector3.up * look.x);
    }
    private void Movement()
    {
        Vector3 moveDir = transform.right * movement.x + transform.forward * movement.y;
        float speedMultiplier = 1f;

        if (movement.y < 0)
            speedMultiplier = 0.5f;
        else if (movement.y > 0)
            speedMultiplier = 1f;

        if (movement.x != 0)
            speedMultiplier = 0.75f;

        if (isGrounded && isSprinting)
        {
            speedMultiplier *= 2f;
        }

        if (!isGrounded)
        {
            speedMultiplier *= 0.5f;
        }

        Vector3 finalMove = moveDir.normalized * movementSpeed * speedMultiplier * Time.deltaTime;
        characterController.Move(finalMove);

        vel.y += gravity * Time.deltaTime;
        characterController.Move(vel * Time.deltaTime);
    }
}