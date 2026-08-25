using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using PrimeTween;

[DisallowMultipleComponent]
public sealed class InspectManager : MonoBehaviour
{
    public static InspectManager Instance { get; private set; }
    public static bool IsInspecting { get; private set; }
    public static event Action<bool> OnInspectStateChanged;

    [Header("References")]
    [SerializeField, Tooltip("관찰 시 물체가 이동할 목표 위치(카메라 앞 빈 게임오브젝트)")] 
    private Transform inspectPoint; 
    [SerializeField] private Light inspectLight; 

    [Header("Settings")]
    public float moveDuration = 0.3f;
    [SerializeField] private float rotationSpeed = 0.5f; 

    public bool IsRotating { get; private set; }
    public InspectableObject CurrentObject { get; private set; } 
    
    private CancellationTokenSource _interactCts;
    private Transform _cachedCamTransform;
    private Sequence _activeSequence;

    private InputAction _rotateAction;
    private InputAction _rotateClickAction;
    private InputAction _cancelAction;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        PrimeTweenConfig.warnEndValueEqualsCurrent = false;

        if (Camera.main is { } mainCam)
        {
            _cachedCamTransform = mainCam.transform;
        }
        else
        {
            Debug.LogError("[InspectManager] 씬에 'MainCamera' 태그가 지정된 카메라가 없습니다!");
        }

        if (inspectLight != null) inspectLight.enabled = false;

        _rotateAction = new InputAction(type: InputActionType.Value, binding: "<Mouse>/delta");
        _rotateClickAction = new InputAction(type: InputActionType.Button, binding: "<Mouse>/leftButton");
        _cancelAction = new InputAction(type: InputActionType.Button, binding: "<Mouse>/rightButton");
    }

    private void OnEnable()
    {
        _cancelAction.performed += OnCancelInput;
        _rotateClickAction.started += OnRotateStarted;
        _rotateClickAction.canceled += OnRotateCanceled;
        
        _rotateAction.Enable(); 
        _rotateClickAction.Enable(); 
        _cancelAction.Enable();
    }

    private void OnDisable()
    {
        _cancelAction.performed -= OnCancelInput;
        _rotateClickAction.started -= OnRotateStarted;
        _rotateClickAction.canceled -= OnRotateCanceled;
        
        _rotateAction.Disable(); 
        _rotateClickAction.Disable(); 
        _cancelAction.Disable();
        
        ResetCancellationToken();
    }

    private void OnDestroy()
    {
        _rotateAction?.Dispose();
        _rotateClickAction?.Dispose();
        _cancelAction?.Dispose();

        if (Instance == this) Instance = null;
    }

    private void OnRotateStarted(InputAction.CallbackContext ctx) => IsRotating = true;
    private void OnRotateCanceled(InputAction.CallbackContext ctx) => IsRotating = false;
    private void OnCancelInput(InputAction.CallbackContext ctx) => StopInspect();

    private void Update()
    {
        if (!IsInspecting || CurrentObject == null || !IsRotating || _cachedCamTransform == null) return;
        
        Vector2 mouseDelta = _rotateAction.ReadValue<Vector2>();
        if (mouseDelta.sqrMagnitude > 0.01f) 
        {
            Transform objTransform = CurrentObject.transform;
            objTransform.RotateAround(objTransform.position, _cachedCamTransform.up, -mouseDelta.x * rotationSpeed);
            objTransform.RotateAround(objTransform.position, _cachedCamTransform.right, mouseDelta.y * rotationSpeed);
        }
    }

    public void StartInspect(InspectableObject obj)
    {
        if (IsInspecting) return;

        // [핵심 방어] 인스펙터 할당 누락으로 인한 NRE 역류 원천 차단
        if (inspectPoint == null)
        {
            Debug.LogError("[InspectManager] 'inspectPoint'가 인스펙터에 할당되지 않았습니다! 상호작용을 중단합니다. (카메라 자식으로 빈 게임오브젝트 생성 후 할당 필요)");
            return;
        }
        
        if (CurrentObject != null && CurrentObject != obj)
        {
            CurrentObject.transform.SetPositionAndRotation(CurrentObject.OriginalPos, CurrentObject.OriginalRot);
            CurrentObject.ResetObject(0f);
        }
        
        CurrentObject = obj; 
        ChangeInspectState(true);

        if (inspectLight != null) inspectLight.enabled = true;
        Cursor.lockState = CursorLockMode.None; 
        Cursor.visible = true;

        ResetCancellationToken();
        Quaternion targetRot = inspectPoint.rotation * Quaternion.Euler(obj.InspectRotationOffset);
        
        MoveObjectAsync(obj.transform, inspectPoint.position, targetRot, _interactCts.Token).Forget();
    }

    public void StopInspect()
    {
        if (!IsInspecting) return;
        ChangeInspectState(false);

        if (inspectLight != null) inspectLight.enabled = false;
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false;

        ResetCancellationToken();
        
        MoveObjectAsync(CurrentObject.transform, CurrentObject.OriginalPos, CurrentObject.OriginalRot, _interactCts.Token).Forget();
    }

    private void ChangeInspectState(bool state)
    {
        IsInspecting = state;
        OnInspectStateChanged?.Invoke(state);
    }

    private void ResetCancellationToken()
{
    // 1. 실행 중인 PrimeTween 트윈 정지
    if (_activeSequence.isAlive)
    {
        _activeSequence.Stop();
    }

    // 2. 널 조건부 연산자(?.)로 안전하게 취소 및 해제 (Null이어도 에러 없이 패스됨)
    _interactCts?.Cancel(); 
    _interactCts?.Dispose();
    
    // 3. 첫 상호작용이든 아니든 무조건 새로운 토큰 소스 발급
    _interactCts = new CancellationTokenSource();
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
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (!IsInspecting && target != null && target.TryGetComponent(out InspectableObject obj) && obj == CurrentObject)
            {
                obj.ResetObject(moveDuration); 
                CurrentObject = null;
            }
        }
    }
}