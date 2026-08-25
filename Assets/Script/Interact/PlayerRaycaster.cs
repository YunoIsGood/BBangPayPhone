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

        for (int i = 0; i < hitCount; i++)
        {
            if (Vector3.Dot(ray.direction, _hits[i].normal) > 0f) continue;

            if (_hits[i].collider.TryGetComponent(out IInteractable interactable))
            {
                if (!interactable.CanInteract) continue;

                if (currentState == GameState.Inspect && currentInspectedT != null)
                {
                    if (!_hits[i].collider.transform.IsChildOf(currentInspectedT)) continue;
                }

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
        var currentState = InteractionStateManager.Instance != null 
            ? InteractionStateManager.Instance.CurrentState 
            : GameState.FPS;

        // 관찰 모드에서 3D 회전 드래그 중일 때만 클릭 무시
        if (currentState == GameState.Inspect && InspectViewer.Instance != null && InspectViewer.Instance.IsRotating)
        {
            return;
        }

        if (CurrentTarget == null) return;

        if (CurrentTarget is UnityEngine.Object unityObj && unityObj == null)
        {
            UpdateTarget(null);
            return;
        }

        IInteractable targetToInteract = CurrentTarget;
        UpdateTarget(null);

        targetToInteract.Interact();
    }
}