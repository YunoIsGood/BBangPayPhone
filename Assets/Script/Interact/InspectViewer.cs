using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using PrimeTween;

[DisallowMultipleComponent]
public sealed class InspectViewer : MonoBehaviour
{
    public static InspectViewer Instance { get; private set; }
    
    public IInspectable CurrentInspectable { get; private set; }
    public bool IsRotating { get; private set; }
    
    // 핵심 추가: 물체가 이동(트윈) 중인지 여부를 반환
    public bool IsTransitioning => _activeSequence.isAlive;

    [Header("References")]
    [SerializeField] private Transform inspectPoint;
    [SerializeField] private Light inspectLight;

    [Header("Settings")]
    [SerializeField] private float moveDuration = 0.3f;
    [SerializeField] private float rotationSpeed = 0.5f;

    private Transform _cachedCamTransform;
    private CancellationTokenSource _cts;
    private Sequence _activeSequence;

    private Vector3 _originalPos;
    private Quaternion _originalRot;

    private InputAction _rotateAction;
    private InputAction _rotateClickAction;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        if (Camera.main is { } mainCam) _cachedCamTransform = mainCam.transform;
        if (inspectLight) inspectLight.enabled = false;

        _rotateAction = new InputAction(type: InputActionType.Value, binding: "<Mouse>/delta");
        _rotateClickAction = new InputAction(type: InputActionType.Button, binding: "<Mouse>/leftButton");
    }

    private void OnEnable()
    {
        _rotateClickAction.started += OnRotateStarted;
        _rotateClickAction.canceled += OnRotateCanceled;
        _rotateAction.Enable(); 
        _rotateClickAction.Enable();
    }

    private void OnDisable()
    {
        _rotateClickAction.started -= OnRotateStarted;
        _rotateClickAction.canceled -= OnRotateCanceled;
        _rotateAction.Disable(); 
        _rotateClickAction.Disable();
        ResetToken();
    }

    private void OnDestroy()
    {
        _rotateAction?.Dispose();
        _rotateClickAction?.Dispose();
        if (Instance == this) Instance = null;
    }

    private void OnRotateStarted(InputAction.CallbackContext ctx)
    {
        if (InteractionStateManager.Instance != null && 
            InteractionStateManager.Instance.CurrentState == GameState.Inspect)
        {
            IsRotating = true;
        }
    }

    private void OnRotateCanceled(InputAction.CallbackContext ctx) => IsRotating = false;

    private void Update()
    {
        if (InteractionStateManager.Instance == null ||
            InteractionStateManager.Instance.CurrentState != GameState.Inspect || 
            CurrentInspectable == null || 
            !IsRotating || 
            _cachedCamTransform == null) return;

        Vector2 mouseDelta = _rotateAction.ReadValue<Vector2>();
        
        if (mouseDelta.sqrMagnitude > 0.01f && mouseDelta.sqrMagnitude < 5000f)
        {
            Transform targetT = CurrentInspectable.ObjectTransform;
            targetT.RotateAround(targetT.position, _cachedCamTransform.up, -mouseDelta.x * rotationSpeed);
            targetT.RotateAround(targetT.position, _cachedCamTransform.right, mouseDelta.y * rotationSpeed);
        }
    }

    public void StartInspect(IInspectable obj)
    {
        if (InteractionStateManager.Instance.CurrentState == GameState.Inspect || obj == null || inspectPoint == null) return;

        CurrentInspectable = obj;
        Transform targetT = obj.ObjectTransform;

        _originalPos = targetT.position;
        _originalRot = targetT.rotation;

        InteractionStateManager.Instance.ChangeState(GameState.Inspect);
        if (inspectLight) inspectLight.enabled = true;

        ResetToken();
        Quaternion targetRot = inspectPoint.rotation * Quaternion.Euler(obj.InspectRotationOffset);
        
        // 이동 애니메이션 시작
        _activeSequence = Sequence.Create()
            .Group(Tween.Position(targetT, inspectPoint.position, moveDuration, Ease.InOutSine))
            .Group(Tween.Rotation(targetT, targetRot, moveDuration, Ease.InOutSine));
    }

    public void StopInspect()
    {
        if (InteractionStateManager.Instance.CurrentState != GameState.Inspect || CurrentInspectable == null) return;

        // 상태를 Focused(선반 줌인)로 되돌림
        InteractionStateManager.Instance.ChangeState(GameState.Focused);
        if (inspectLight) inspectLight.enabled = false;

        ResetToken();
        Transform targetT = CurrentInspectable.ObjectTransform;

        // 물체가 제자리로 돌아가는 애니메이션 실행
        _activeSequence = Sequence.Create()
            .Group(Tween.Position(targetT, _originalPos, moveDuration, Ease.InOutSine))
            .Group(Tween.Rotation(targetT, _originalRot, moveDuration, Ease.InOutSine))
            .OnComplete(() => CurrentInspectable = null);
    }

    private void ResetToken()
    {
        if (_activeSequence.isAlive) _activeSequence.Stop();
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
    }
}