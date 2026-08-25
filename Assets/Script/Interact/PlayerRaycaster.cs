using System;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public sealed class PlayerRaycaster : MonoBehaviour
{
    public event Action<IInteractable> OnTargetChanged;
    public IInteractable CurrentTarget { get; private set; }

    [Header("Settings")]
    [SerializeField] private float interactDistance = 5f;
    [SerializeField] private LayerMask interactLayer;
    [SerializeField] private float depthTolerance = 0.15f; 

    private Camera _cachedCam;
    private readonly RaycastHit[] _hits = new RaycastHit[10]; 
    private InputAction _interactAction;

    private void Awake()
    {
        _cachedCam = Camera.main;
        _interactAction = new InputAction(name: "Interact", type: InputActionType.Button, binding: "<Mouse>/leftButton");
    }

    private void OnEnable()
    {
        _interactAction.performed += OnInteractPerformed;
        _interactAction.Enable();
        InspectManager.OnInspectStateChanged += HandleInspectStateChanged;
    }

    private void OnDisable()
    {
        _interactAction.performed -= OnInteractPerformed;
        _interactAction.Disable();
        InspectManager.OnInspectStateChanged -= HandleInspectStateChanged;
    }

    private void OnDestroy()
    {
        _interactAction?.Dispose();
    }

    // 관찰 상태 진입/해제 시 타겟을 즉시 비웁니다.
    private void HandleInspectStateChanged(bool isInspecting) => UpdateTarget(null);

    private void Update()
    {
        if (_cachedCam == null) return;

        bool isInspecting = InspectManager.IsInspecting;
        Vector2 pointerPos = Mouse.current?.position.ReadValue() ?? new Vector2(Screen.width / 2f, Screen.height / 2f);

        Ray ray = isInspecting 
            ? _cachedCam.ScreenPointToRay(pointerPos) 
            : new Ray(_cachedCam.transform.position, _cachedCam.transform.forward);

        float currentMaxDistance = isInspecting ? 100f : interactDistance;
        int hitCount = Physics.RaycastNonAlloc(ray, _hits, currentMaxDistance, interactLayer);
        
        InspectableObject currentInspectedObj = InspectManager.Instance?.CurrentObject;
        float parentHitDistance = float.MaxValue;
        
        for (int i = 0; i < hitCount; i++)
        {
            if (Vector3.Dot(ray.direction, _hits[i].normal) > 0f) continue;
            
            if (_hits[i].collider.TryGetComponent(out InspectableObject parentObj))
            {
                if (isInspecting && currentInspectedObj != null && parentObj != currentInspectedObj) continue; 
                if (_hits[i].distance < parentHitDistance) parentHitDistance = _hits[i].distance;
            }
        }

        IInteractable validTarget = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            if (Vector3.Dot(ray.direction, _hits[i].normal) > 0f) continue;
            
            if (_hits[i].collider.TryGetComponent(out IInteractable interactable))
            {
                if (!interactable.CanInteract) continue;
                if (isInspecting && currentInspectedObj != null && !_hits[i].collider.transform.IsChildOf(currentInspectedObj.transform)) continue;
                if (interactable is InspectablePart && _hits[i].distance > parentHitDistance + depthTolerance) continue; 

                if (_hits[i].distance < closestDistance)
                {
                    validTarget = interactable;
                    closestDistance = _hits[i].distance;
                }
            }
        }

        UpdateTarget(validTarget);
    }

    private void UpdateTarget(IInteractable newTarget)
    {
        if (CurrentTarget != newTarget)
        {
            CurrentTarget = newTarget;
            OnTargetChanged?.Invoke(CurrentTarget);
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        if (InspectManager.Instance != null && InspectManager.Instance.IsRotating) return;
        
        // 1. 타겟이 없으면 조기 종료
        if (CurrentTarget == null) return;

        // 2. Fake Null 방어
        if (CurrentTarget is UnityEngine.Object unityObj && unityObj == null)
        {
            UpdateTarget(null);
            return;
        }
        
        // 3. 상호작용 실행 시 임시로 캐싱 후, 상태 변경 전에 타겟을 먼저 해제하여 콜백 순서 꼬임을 방지합니다.
        IInteractable targetToInteract = CurrentTarget;
        UpdateTarget(null); 
        
        targetToInteract.Interact();
    }
}