using Script.Interact;
using Script.Player.Module;
using UnityEngine;

// FPS 모드에서 이 구역을 클릭하면 설정된 카메라 위치로 줌인됩니다.
public sealed class FocusZone : MonoBehaviour, IInteractable
{
    //이거 싱글톤 안쓰기 위해 임시로 저렇게 달아놓긴 했는데 이렇게 모듈 하나하나 가져오는거 너무 맘에 안드는데
    //코드 구조 추천 부탁
    [SerializeField] private CameraControlModule cameraControlModule;
    
    [SerializeField, Tooltip("줌인될 카메라의 목표 위치/회전값")] 
    private Transform targetCameraView;

    // FPS 상태일 때만 이 구역을 클릭해 줌인할 수 있음
    public bool CanInteract => InteractionStateManager.Instance.CurrentState == GameState.FPS;

    public void Interact()
    {
        if (CanInteract) cameraControlModule.MoveToZone(targetCameraView);
    }
}