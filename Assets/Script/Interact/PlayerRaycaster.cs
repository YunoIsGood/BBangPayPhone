using System;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public sealed class PlayerRaycaster : MonoBehaviour
{
    public event Action<IInteractable> OnTargetChanged;
    public IInteractable CurrentTarget { get; private set; }

    [Header("Settings")]
    [SerializeField] private float fpsInteractDistance = 15f;
    [SerializeField] private float focusInteractDistance = 10f;
    [SerializeField] private LayerMask interactLayer;

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

        if (InteractionStateManager.Instance != null)
        {
            InteractionStateManager.Instance.OnStateChanged += HandleStateChanged;
        }
    }

    private void OnDisable()
    {
        _interactAction.performed -= OnInteractPerformed;
        _interactAction.Disable();

        if (InteractionStateManager.Instance != null)
        {
            InteractionStateManager.Instance.OnStateChanged -= HandleStateChanged;
        }
    }

    private void OnDestroy()
    {
        _interactAction?.Dispose();
    }

    private void HandleStateChanged(GameState state)
    {
        UpdateTarget(null);
    }

    private void Update()
    {
        if (_cachedCam == null || InteractionStateManager.Instance == null) return;

        var currentState = InteractionStateManager.Instance.CurrentState;

        if (currentState == GameState.UI)
        {
            UpdateTarget(null);
            return;
        }

        bool isMousePointerMode = (currentState == GameState.Focused || currentState == GameState.Inspect);

        Vector2 pointerPos = isMousePointerMode
            ? (Mouse.current?.position.ReadValue() ?? new Vector2(Screen.width / 2f, Screen.height / 2f))
            : new Vector2(Screen.width / 2f, Screen.height / 2f);

        Ray ray = isMousePointerMode
            ? _cachedCam.ScreenPointToRay(pointerPos)
            : new Ray(_cachedCam.transform.position, _cachedCam.transform.forward);

        float maxDistance = isMousePointerMode ? focusInteractDistance : fpsInteractDistance;
        int hitCount = Physics.RaycastNonAlloc(ray, _hits, maxDistance, interactLayer);

        Transform currentInspectedT = InspectViewer.Instance?.CurrentInspectable?.ObjectTransform;

        IInteractable validTarget = null;
        float closestDistance = float.MaxValue;
        bool foundPart = false; // 자식 파트를 찾았는지 추적

        for (int i = 0; i < hitCount; i++)
        {
            if (Vector3.Dot(ray.direction, _hits[i].normal) > 0f) continue;

            if (_hits[i].collider.TryGetComponent(out IInteractable interactable))
            {
                if (!interactable.CanInteract) continue;

                // 360도 관찰 중일 때의 특수 처리
                if (currentState == GameState.Inspect && currentInspectedT != null)
                {
                    // 현재 관찰 중인 물체의 자식이 아니면 무시
                    if (!_hits[i].collider.transform.IsChildOf(currentInspectedT) && _hits[i].collider.transform != currentInspectedT)
                    {
                        continue;
                    }

                    // 🚨 핵심 로직: 부품(InspectablePart)이 부모(InspectableObject)보다 더 깊이 있어도 무조건 우선순위를 가짐
                    if (interactable is InspectablePart)
                    {
                        if (!foundPart || _hits[i].distance < closestDistance)
                        {
                            validTarget = interactable;
                            closestDistance = _hits[i].distance;
                            foundPart = true;
                        }
                    }
                    else if (!foundPart && _hits[i].distance < closestDistance)
                    {
                        // 부품을 아직 못 찾았을 때만 부모 물체를 타깃으로 삼음
                        validTarget = interactable;
                        closestDistance = _hits[i].distance;
                    }
                }
                // FPS나 Focused 상태일 때의 일반 거리 계산
                else if (_hits[i].distance < closestDistance)
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
        if (CurrentTarget == null) return;

        // Fake Null 방어
        if (CurrentTarget is UnityEngine.Object unityObj && unityObj == null)
        {
            UpdateTarget(null);
            return;
        }

        var currentState = InteractionStateManager.Instance != null 
            ? InteractionStateManager.Instance.CurrentState 
            : GameState.FPS;

        // 🚨 핵심 수정: 360도 관찰 모드일 때의 클릭 로직 완벽 분리
        if (currentState == GameState.Inspect)
        {
            if (CurrentTarget is InspectablePart)
            {
                // 타깃이 '단추/부품'이라면, 마우스 회전 플래그(IsRotating)와 상관없이 무조건 클릭 통과!
            }
            else
            {
                // 타깃이 부품이 아니라면 (그냥 지갑 본체나 허공), 
                // 이것은 360도 회전을 위한 드래그 클릭이므로 상호작용 실행을 여기서 중단합니다.
                return;
            }
        }

        // 클릭 실행
        IInteractable targetToInteract = CurrentTarget;
        UpdateTarget(null);
        targetToInteract.Interact();
    }
}