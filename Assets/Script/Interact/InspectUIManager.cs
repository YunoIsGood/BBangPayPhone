using UnityEngine;

[DisallowMultipleComponent]
public sealed class InspectUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerRaycaster playerRaycaster;

    [Header("FPS Crosshair UI")]
    [SerializeField] private GameObject defaultCrosshair;
    [SerializeField] private GameObject interactCrosshair;

    [Header("Hardware Cursors (32x32)")]
    [SerializeField, Tooltip("기본 마우스 커서 (텍스처 타입: Cursor)")] 
    private Texture2D inspectDefaultCursor;
    [SerializeField] private Vector2 defaultCursorHotspot = Vector2.zero;

    [SerializeField, Tooltip("상호작용 가능한 물체 위에 올렸을 때 커서")] 
    private Texture2D interactPartCursor;
    [SerializeField] private Vector2 partCursorHotspot = new Vector2(12f, 2f); // 손가락 끝 픽셀 좌표

    private void Awake()
    {
        if (defaultCrosshair) defaultCrosshair.SetActive(true);
        if (interactCrosshair) interactCrosshair.SetActive(false);
    }

    private void OnEnable()
    {
        if (InteractionStateManager.Instance != null)
        {
            InteractionStateManager.Instance.OnStateChanged += HandleStateChanged;
        }

        if (playerRaycaster != null)
        {
            playerRaycaster.OnTargetChanged += HandleTargetChanged;
        }
    }

    private void OnDisable()
    {
        if (InteractionStateManager.Instance != null)
        {
            InteractionStateManager.Instance.OnStateChanged -= HandleStateChanged;
        }

        if (playerRaycaster != null)
        {
            playerRaycaster.OnTargetChanged -= HandleTargetChanged;
        }
    }

    private void HandleStateChanged(GameState state)
    {
        bool isFPS = state == GameState.FPS;

        // 1. 크로스헤어 UI 제어
        if (defaultCrosshair) defaultCrosshair.SetActive(isFPS);
        if (interactCrosshair) interactCrosshair.SetActive(false);

        // 2. 하드웨어 커서 초기화
        if (isFPS)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(inspectDefaultCursor, defaultCursorHotspot, CursorMode.Auto);
        }
    }

    private void HandleTargetChanged(IInteractable newTarget)
    {
        var currentState = InteractionStateManager.Instance != null 
            ? InteractionStateManager.Instance.CurrentState 
            : GameState.FPS;

        bool hasTarget = newTarget != null && newTarget.CanInteract;

        // 1. FPS 모드일 때 크로스헤어 변경
        if (currentState == GameState.FPS)
        {
            if (defaultCrosshair) defaultCrosshair.SetActive(!hasTarget);
            if (interactCrosshair) interactCrosshair.SetActive(hasTarget);
            return;
        }

        // 2. Focused(줌인) 또는 Inspect(360도 관찰) 모드일 때 하드웨어 커서 변경
        if (currentState == GameState.Focused || currentState == GameState.Inspect)
        {
            if (hasTarget)
            {
                Cursor.SetCursor(interactPartCursor, partCursorHotspot, CursorMode.Auto);
            }
            else
            {
                Cursor.SetCursor(inspectDefaultCursor, defaultCursorHotspot, CursorMode.Auto);
            }
        }
    }
}