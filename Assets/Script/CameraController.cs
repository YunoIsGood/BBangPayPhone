using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;
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
    private Sequence _cameraSequence;
    
    // 🚨 쿨타임 측정용 변수 (더블 트리거 방지)
    private float _lastInspectTime = 0f; 

    private void Awake()
    {
        PrimeTweenConfig.warnEndValueEqualsCurrent = false; 
        Instance = this;
        _fpsOriginPos = cameraTransform.position;
        _fpsOriginRot = cameraTransform.rotation;
        
        SyncRotationVariables();
        _lookAction = new InputAction(type: InputActionType.Value, binding: "<Mouse>/delta");
    }

    private void OnEnable() 
    {
        _lookAction.Enable();
        if (InteractionStateManager.Instance != null)
            InteractionStateManager.Instance.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        _lookAction.Disable();
        if (_cameraSequence.isAlive) _cameraSequence.Stop();
        if (InteractionStateManager.Instance != null)
            InteractionStateManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    // 🚨 관찰 모드(Inspect)로 진입한 시간을 기록
    private void HandleStateChanged(GameState state)
    {
        if (state == GameState.Inspect) _lastInspectTime = Time.time;
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            HandleRightClick();
        }
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

    private void HandleRightClick()
    {
        var currentState = InteractionStateManager.Instance.CurrentState;

        // 🚨 핵심 방어막: 관찰 모드가 열린 지 0.2초가 안 지났다면 닫기 명령 완벽 무시
        if (currentState == GameState.Inspect && Time.time - _lastInspectTime < 0.2f) return;

        if (IsPointerOverUI()) return;

        if (_cameraSequence.isAlive) return;
        if (InspectViewer.Instance != null && InspectViewer.Instance.IsTransitioning) return;

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
        else if (currentState == GameState.Inspect)
        {
            if (InspectViewer.Instance != null) InspectViewer.Instance.StopInspect();
        }
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        PointerEventData eventData = new PointerEventData(EventSystem.current) { position = Mouse.current?.position.ReadValue() ?? Vector2.zero };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    private void SyncRotationVariables()
    {
        Vector3 angles = cameraTransform.eulerAngles;
        float normalizedX = angles.x > 180f ? angles.x - 360f : angles.x;
        _xRotation = Mathf.Clamp(normalizedX, minPitch, maxPitch);
        _yRotation = angles.y;
    }
}