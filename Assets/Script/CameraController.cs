using UnityEngine;
using UnityEngine.InputSystem;
using PrimeTween;
using System;

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

    private void Awake()
    {
        Instance = this;
        _fpsOriginPos = cameraTransform.position;
        _fpsOriginRot = cameraTransform.rotation;
        _yRotation = cameraTransform.eulerAngles.y;

        _lookAction = new InputAction(type: InputActionType.Value, binding: "<Mouse>/delta");
        _cancelAction = new InputAction(type: InputActionType.Button, binding: "<Mouse>/rightButton");
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
    }

    private void LateUpdate()
    {
        // FPS 상태일 때만 마우스로 고개를 돌릴 수 있음
        if (InteractionStateManager.Instance.CurrentState != GameState.FPS) return;

        Vector2 lookInput = _lookAction.ReadValue<Vector2>();
        if (lookInput.sqrMagnitude < 0.001f) return;

        _xRotation = Mathf.Clamp(_xRotation - (lookInput.y * lookSensitivity), minPitch, maxPitch);
        _yRotation += lookInput.x * lookSensitivity;

        cameraTransform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0f);
        _fpsOriginRot = cameraTransform.rotation; // 마지막으로 바라본 방향 저장
    }

    // 특정 구역(FocusZone)을 클릭했을 때 카메라 이동
    public void MoveToZone(Transform targetViewPoint)
    {
        InteractionStateManager.Instance.ChangeState(GameState.Focused);
        
        Sequence.Create()
            .Group(Tween.Position(cameraTransform, targetViewPoint.position, moveDuration, Ease.InOutSine))
            .Group(Tween.Rotation(cameraTransform, targetViewPoint.rotation, moveDuration, Ease.InOutSine));
    }

    // 우클릭(Cancel) 시 이전 상태로 복귀
    private void OnCancelPerformed(InputAction.CallbackContext ctx)
    {
        var currentState = InteractionStateManager.Instance.CurrentState;

        if (currentState == GameState.Focused)
        {
            // 줌인 상태에서 우클릭 -> FPS 기본 위치로 복귀
            InteractionStateManager.Instance.ChangeState(GameState.FPS);
            Sequence.Create()
                .Group(Tween.Position(cameraTransform, _fpsOriginPos, moveDuration, Ease.InOutSine))
                .Group(Tween.Rotation(cameraTransform, _fpsOriginRot, moveDuration, Ease.InOutSine));
        }
        else if (currentState == GameState.Inspect)
        {
            // 360도 관찰 중 우클릭 -> 물건 내려놓기 (InspectViewer에서 처리)
            InspectViewer.Instance.StopInspect();
        }
    }
}