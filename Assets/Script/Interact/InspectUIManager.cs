using UnityEngine;

[DisallowMultipleComponent]
public sealed class InspectUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerRaycaster playerRaycaster;

    [Header("FPS Crosshair UI")]
    [SerializeField] private GameObject defaultCrosshair;
    [SerializeField] private GameObject interactCrosshair;

    [Header("Inspect Hardware Cursors")]
    [SerializeField, Tooltip("관찰 모드 기본 커서 (텍스처 타입: Cursor)")] 
    private Texture2D inspectDefaultCursor;
    [SerializeField, Tooltip("관찰 모드 중 상호작용 가능한 파트 위에 올렸을 때 커서")] 
    private Texture2D inspectPartCursor;
    [SerializeField, Tooltip("커서의 클릭 판정 중심점 (보통 좌상단이면 0,0)")] 
    private Vector2 cursorHotspot = Vector2.zero;

    private void Awake()
    {
        if (defaultCrosshair) defaultCrosshair.SetActive(true);
        if (interactCrosshair) interactCrosshair.SetActive(false);
    }

    private void OnEnable()
    {
        InspectManager.OnInspectStateChanged += HandleInspectStateChanged;
        if (playerRaycaster) playerRaycaster.OnTargetChanged += HandleTargetChanged;
    }

    private void OnDisable()
    {
        InspectManager.OnInspectStateChanged -= HandleInspectStateChanged;
        if (playerRaycaster) playerRaycaster.OnTargetChanged -= HandleTargetChanged;
    }

    private void HandleInspectStateChanged(bool isInspecting)
    {
        if (isInspecting)
        {
            // 관찰 모드 진입: 화면 중앙 크로스헤어 끄기 및 기본 하드웨어 커서 활성화
            if (defaultCrosshair) defaultCrosshair.SetActive(false);
            if (interactCrosshair) interactCrosshair.SetActive(false);
            Cursor.SetCursor(inspectDefaultCursor, cursorHotspot, CursorMode.Auto);
        }
        else
        {
            // 관찰 모드 해제: 크로스헤어 켜기 및 하드웨어 커서 비활성화
            if (defaultCrosshair) defaultCrosshair.SetActive(true);
            if (interactCrosshair) interactCrosshair.SetActive(false);
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }

    private void HandleTargetChanged(IInteractable newTarget)
    {
        // 1. 일반 FPS 탐색 모드 (화면 중앙 크로스헤어 처리)
        if (!InspectManager.IsInspecting)
        {
            bool hasTarget = newTarget != null;
            if (defaultCrosshair) defaultCrosshair.SetActive(!hasTarget);
            if (interactCrosshair) interactCrosshair.SetActive(hasTarget);
            return;
        }

        // 2. 관찰(Inspect) 모드 중 마우스 오버 처리 (하드웨어 커서 처리)
        if (newTarget is InspectablePart)
        {
            // 파트 위에 올렸을 때의 특수 커서 (예: 톱니바퀴, 돋보기 아이콘 등)
            Cursor.SetCursor(inspectPartCursor, cursorHotspot, CursorMode.Auto);
        }
        else
        {
            // 빈 공간이거나 그냥 물체(InspectableObject) 위일 때
            Cursor.SetCursor(inspectDefaultCursor, cursorHotspot, CursorMode.Auto);
        }
    }
}