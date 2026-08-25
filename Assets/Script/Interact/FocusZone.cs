using UnityEngine;

// FPS 모드에서 이 구역을 클릭하면 설정된 카메라 위치로 줌인됩니다.
public sealed class FocusZone : MonoBehaviour, IInteractable
{
    [SerializeField, Tooltip("줌인될 카메라의 목표 위치/회전값")] 
    private Transform targetCameraView;

    // FPS 상태일 때만 이 구역을 클릭해 줌인할 수 있음
    public bool CanInteract => InteractionStateManager.Instance.CurrentState == GameState.FPS;

    public void Interact()
    {
        if (CanInteract) CameraController.Instance.MoveToZone(targetCameraView);
    }
}