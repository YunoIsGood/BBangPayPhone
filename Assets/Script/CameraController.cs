using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public sealed class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    [Header("Rotation Limits")]
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 30f;
    [SerializeField] private float minYaw = -40f;
    [SerializeField] private float maxYaw = 40f;

    [Header("Sensitivity")]
    [SerializeField] private float lookSensitivity = 0.5f;

    private float _xRotation = 0f;
    private float _yRotation = 0f;
    private InputAction _lookAction;

    private void Awake()
    {
        if (!cameraTransform) cameraTransform = transform;

        _lookAction = new InputAction(name: "Look", type: InputActionType.Value, binding: "<Mouse>/delta");
    }

    private void OnEnable() => _lookAction?.Enable();
    private void OnDisable() => _lookAction?.Disable();

    private void OnDestroy()
    {
        // Dynamic InputAction 네이티브 해제
        _lookAction?.Dispose();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate() 
    {
        if (InspectManager.IsInspecting || _lookAction == null) return;

        Vector2 lookInput = _lookAction.ReadValue<Vector2>();

        if (lookInput.sqrMagnitude < 0.001f) return;

        _xRotation = Mathf.Clamp(_xRotation - (lookInput.y * lookSensitivity), minPitch, maxPitch);
        _yRotation = Mathf.Clamp(_yRotation + (lookInput.x * lookSensitivity), minYaw, maxYaw);

        cameraTransform.localRotation = Quaternion.Euler(_xRotation, _yRotation, 0f);
    }
}