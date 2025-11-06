using UnityEngine;
using UnityEngine.InputSystem;

public class PickUpObjectController : MonoBehaviour
{
    public float PickUpCheckDistance = 2f;
    public float holdDistance = 1.5f;
    public float holdSmoothness = 10f;

    public float minHoldDistance = 1f;
    public float maxHoldDistance = 6f;
    public float scrollSensitivity = 1f;

    public LayerMask Pickup;
    private Camera playerCamera;

    private Rigidbody heldObject;
    private PlayerInputs inputAction;
    private bool isHolding;

    private void Awake()
    {
        inputAction = new PlayerInputs();
    }

    void Start()
    {
        inputAction.Player.Enable();
        playerCamera = Camera.main;
    }

    void Update()
    {
        float scrollInput = inputAction.Player.Scroll.ReadValue<Vector2>().y;
        if (isHolding && Mathf.Abs(scrollInput) > 0.01f)
        {
            AdjustHoldDistance(scrollInput);
        }

        if (inputAction.Player.PickUp.IsPressed())
        {
            if (!isHolding)
            {
                TryPickUpObject();
            }
            else
            {
                HoldObject();
            }
        }
        else
        {
            if (isHolding)
            {
                DropObject();
            }
        }
    }

    private void TryPickUpObject()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, PickUpCheckDistance, Pickup))
        {
            Rigidbody rb = hit.collider.attachedRigidbody;
            if (rb != null)
            {
                heldObject = rb;
                heldObject.useGravity = false;
                heldObject.linearVelocity = Vector3.zero;
                isHolding = true;
            }
        }
        Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * PickUpCheckDistance, Color.red);
    }

    private void HoldObject()
    {
        if (heldObject == null) return;

        Vector3 targetPos = playerCamera.transform.position + playerCamera.transform.forward * holdDistance;
        Vector3 moveDirection = (targetPos - heldObject.position);
        heldObject.linearVelocity = moveDirection * holdSmoothness;

        heldObject.MoveRotation(Quaternion.Slerp(
            heldObject.rotation,
            playerCamera.transform.rotation,
            Time.deltaTime * holdSmoothness
        ));
    }

    private void DropObject()
    {
        if (heldObject == null) return;

        heldObject.useGravity = true;
        heldObject = null;
        isHolding = false;
    }
    private void AdjustHoldDistance(float scrollValue)
    {
        holdDistance += scrollValue * scrollSensitivity * Time.deltaTime * 100f;
        holdDistance = Mathf.Clamp(holdDistance, minHoldDistance, maxHoldDistance);
    }
}