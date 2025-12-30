using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")] [SerializeField] private float moveSpeed = 10.0f;
    [SerializeField] private bool normalizeInput = true;
    [SerializeField] private InputActionReference moveAction;

    [Header("Rotation")] [SerializeField] private float sensitivity = 0.2f;
    [SerializeField] private Constraint verticalConstraint = new (-90.0f, 70.0f);

    [Header("Misc.")] [SerializeField] private bool disableCursor = true;

    private CharacterController _characterController;
    private Camera _camera;
    private Vector2 _prevMousePosition;
    private float _verticalAngle;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();

        _camera = GetComponent<Camera>();
        if (_camera == null)
        {
            _camera = GetComponentInChildren<Camera>();
        }

        if (_camera == null)
        {
            _camera = Camera.main;
        }

        if (_camera == null)
        {
            Debug.LogError("Player Controller: Camera not found!");
        }
    }

    private void Start()
    {
        if (disableCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void Update()
    {
        UpdatePosition();
        UpdateRotation();
    }

    private void UpdatePosition()
    {
        var moveInput = moveAction.action.ReadValue<Vector2>();
        var moveDir = new Vector3(moveInput.x, 0.0f, moveInput.y);
        if (normalizeInput)
        {
            moveDir.Normalize();
        }

        var moveVec = moveDir * moveSpeed;
        var relativeMoveVec = moveVec.x * transform.right + moveVec.y * transform.up + moveVec.z * transform.forward;
        
        _characterController.Move(relativeMoveVec * Time.deltaTime);
    }

    private void UpdateRotation()
    {
        var mouseDelta = Mouse.current.delta.ReadValue();

        var scaledMouseDelta = mouseDelta * sensitivity;
        var clampedMouseDelta = new Vector2(scaledMouseDelta.x, verticalConstraint.Clamp(_verticalAngle + scaledMouseDelta.y) - _verticalAngle);
        _verticalAngle += clampedMouseDelta.y;

        transform.Rotate(Vector3.up, clampedMouseDelta.x);
        _camera.transform.Rotate(Vector3.left, clampedMouseDelta.y);
    }
}