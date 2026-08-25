using UnityEngine;
using UnityEngine.InputSystem;
using PrimeTween;

[DisallowMultipleComponent]
public sealed class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float moveDuration = 0.5f;

    [Header("FPS Rotation Limits")]
    [SerializeField] private float minPitch = -45f;
    [SerializeField] private float maxPitch = 45f;
    [SerializeField] private float lookSensitivity = 0.5f;

    private float _xRotation = 0f;
    private float _yRotation = 0f;
    
    private Vector3 _fpsOriginPos;
    private Quaternion _fpsOriginRot;

    private InputAction _lookAction;
    private InputAction _cancelAction;
    private Sequence _cameraSequence;

    private void Awake()
    {
        Instance = this;
        Cursor.lockState = CursorLockMode.Locked;
        _fpsOriginPos = cameraTransform.position;
        _fpsOriginRot = cameraTransform.rotation;
        
        SyncRotationVariables();

        _lookAction = new InputAction(type: InputActionType.Value, binding: "<Mouse>/delta");
        _cancelAction = new InputAction(type: InputActionType.Button, binding: "<Mouse>/rightButton");
    }

    public void DebugSystem()
    {
        Debug.Log("상호작용됨!");   
    }

    private void OnEnable()
    {
        _lookAction.Enable();
        _cancelAction.performed += OnCancelPerformed;
        _cancelAction.Enable();
    }

    private void OnDisable()
    {
        _lookAction.Disable();
        _cancelAction.Disable();
        if (_cameraSequence.isAlive) _cameraSequence.Stop();
    }

    private void LateUpdate()
    {
        if (InteractionStateManager.Instance.CurrentState != GameState.FPS || _cameraSequence.isAlive) return;

        Vector2 lookInput = _lookAction.ReadValue<Vector2>();
        if (lookInput.sqrMagnitude < 0.01f || lookInput.sqrMagnitude > 10000f) return;

        _xRotation = Mathf.Clamp(_xRotation - (lookInput.y * lookSensitivity), minPitch, maxPitch);
        _yRotation += lookInput.x * lookSensitivity;

        cameraTransform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0f);
    }

    public void MoveToZone(Transform targetViewPoint)
    {
        if (_cameraSequence.isAlive) return;

        _fpsOriginPos = cameraTransform.position;
        _fpsOriginRot = cameraTransform.rotation;

        InteractionStateManager.Instance.ChangeState(GameState.Focused);
        
        _cameraSequence = Sequence.Create()
            .Group(Tween.Position(cameraTransform, targetViewPoint.position, moveDuration, Ease.InOutSine))
            .Group(Tween.Rotation(cameraTransform, targetViewPoint.rotation, moveDuration, Ease.InOutSine));
    }

    private void OnCancelPerformed(InputAction.CallbackContext ctx)
    {
        // 🚨 핵심 수정: 카메라가 이동 중이거나, 물체가 날아가고 있는 중에는 우클릭 완전 무시! (더블 트리거 방지)
        if (_cameraSequence.isAlive) return;
        if (InspectViewer.Instance != null && InspectViewer.Instance.IsTransitioning) return;

        var currentState = InteractionStateManager.Instance.CurrentState;

        // 1. FocusZone(줌인) 상태에서 우클릭 -> FPS(기본 1인칭) 상태로 복귀
        if (currentState == GameState.Focused)
        {
            _cameraSequence = Sequence.Create()
                .Group(Tween.Position(cameraTransform, _fpsOriginPos, moveDuration, Ease.InOutSine))
                .Group(Tween.Rotation(cameraTransform, _fpsOriginRot, moveDuration, Ease.InOutSine))
                .OnComplete(() => 
                {
                    SyncRotationVariables();
                    InteractionStateManager.Instance.ChangeState(GameState.FPS);
                });
        }
        // 2. Inspect(360도 관찰) 상태에서 우클릭 -> 뷰어에게 물건을 내려놓으라고 지시 (카메라는 움직이지 않음)
        else if (currentState == GameState.Inspect)
        {
            if (InspectViewer.Instance != null)
            {
                InspectViewer.Instance.StopInspect();
            }
        }
    }

    private void SyncRotationVariables()
    {
        Vector3 angles = cameraTransform.eulerAngles;
        float normalizedX = angles.x > 180f ? angles.x - 360f : angles.x;
        
        _xRotation = Mathf.Clamp(normalizedX, minPitch, maxPitch);
        _yRotation = angles.y;
    }
}