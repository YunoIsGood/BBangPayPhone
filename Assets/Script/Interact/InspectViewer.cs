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

    [Header("References")]
    [SerializeField, Tooltip("카메라 앞 관찰 포인트 빈 오브젝트")] 
    private Transform inspectPoint;
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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (Camera.main is { } mainCam)
        {
            _cachedCamTransform = mainCam.transform;
        }

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

    // 관찰(Inspect) 모드가 아닐 때는 IsRotating이 켜지지 않도록 차단
    private void OnRotateStarted(InputAction.CallbackContext ctx)
    {
        if (InteractionStateManager.Instance != null && 
            InteractionStateManager.Instance.CurrentState == GameState.Inspect)
        {
            IsRotating = true;
        }
    }

    private void OnRotateCanceled(InputAction.CallbackContext ctx)
    {
        IsRotating = false;
    }

    private void Update()
    {
        if (InteractionStateManager.Instance == null ||
            InteractionStateManager.Instance.CurrentState != GameState.Inspect || 
            CurrentInspectable == null || 
            !IsRotating || 
            _cachedCamTransform == null) return;

        Vector2 mouseDelta = _rotateAction.ReadValue<Vector2>();
        if (mouseDelta.sqrMagnitude > 0.01f)
        {
            Transform targetT = CurrentInspectable.ObjectTransform;
            targetT.RotateAround(targetT.position, _cachedCamTransform.up, -mouseDelta.x * rotationSpeed);
            targetT.RotateAround(targetT.position, _cachedCamTransform.right, mouseDelta.y * rotationSpeed);
        }
    }

    public void StartInspect(IInspectable obj)
    {
        if (InteractionStateManager.Instance.CurrentState == GameState.Inspect || obj == null) return;

        if (inspectPoint == null)
        {
            Debug.LogError("[InspectViewer] 'inspectPoint'가 할당되지 않았습니다! 카메라 자식으로 빈 오브젝트를 생성해 할당하세요.");
            return;
        }

        CurrentInspectable = obj;
        Transform targetT = obj.ObjectTransform;

        _originalPos = targetT.position;
        _originalRot = targetT.rotation;

        InteractionStateManager.Instance.ChangeState(GameState.Inspect);
        if (inspectLight) inspectLight.enabled = true;

        ResetToken();
        Quaternion targetRot = inspectPoint.rotation * Quaternion.Euler(obj.InspectRotationOffset);
        MoveObjectAsync(targetT, inspectPoint.position, targetRot, _cts.Token).Forget();
    }

    public void StopInspect()
    {
        if (InteractionStateManager.Instance.CurrentState != GameState.Inspect || CurrentInspectable == null) return;

        InteractionStateManager.Instance.ChangeState(GameState.Focused);
        if (inspectLight) inspectLight.enabled = false;

        ResetToken();
        Transform targetT = CurrentInspectable.ObjectTransform;

        MoveObjectAsync(targetT, _originalPos, _originalRot, _cts.Token)
            .ContinueWith(() => CurrentInspectable = null)
            .Forget();
    }

    private void ResetToken()
    {
        if (_activeSequence.isAlive) _activeSequence.Stop();
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
    }

    private async UniTask MoveObjectAsync(Transform target, Vector3 toPos, Quaternion toRot, CancellationToken token)
    {
        try
        {
            _activeSequence = Sequence.Create()
                .Group(Tween.Position(target, toPos, moveDuration, Ease.InOutSine))
                .Group(Tween.Rotation(target, toRot, moveDuration, Ease.InOutSine));

            await _activeSequence;
        }
        catch (OperationCanceledException) { }
    }
}