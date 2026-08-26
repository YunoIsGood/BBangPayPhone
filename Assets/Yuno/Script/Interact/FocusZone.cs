using Script.Interact;
using Script.Player.Module;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public sealed class FocusZone : MonoBehaviour, IInteractable
{
<<<<<<< Updated upstream:Assets/Yuno/Script/Interact/FocusZone.cs
    //이거 싱글톤 안쓰기 위해 임시로 저렇게 달아놓긴 했는데 이렇게 모듈 하나하나 가져오는거 너무 맘에 안드는데
    //코드 구조 추천 부탁
    [SerializeField] private CameraControlModule cameraControlModule;
    
    [SerializeField, Tooltip("줌인될 카메라의 목표 위치/회전값")] 
=======
    [SerializeField, Tooltip("줌인될 카메라의 목표 위치/회전값 (자식 빈 오브젝트)")] 
>>>>>>> Stashed changes:Assets/Script/Interact/FocusZone.cs
    private Transform targetCameraView;

    public bool CanInteract => InteractionStateManager.Instance != null && 
                               InteractionStateManager.Instance.CurrentState == GameState.FPS;

    public void Interact()
    {
<<<<<<< Updated upstream:Assets/Yuno/Script/Interact/FocusZone.cs
        if (CanInteract) cameraControlModule.MoveToZone(targetCameraView);
=======
        if (!CanInteract) return;

        // 🚨 줌인 실행 전, 이 FocusZone을 현재 활성 구역으로 시스템에 등록
        InteractionStateManager.Instance.SetFocusZone(transform);
        
        if (CameraController.Instance != null && targetCameraView != null)
        {
            CameraController.Instance.MoveToZone(targetCameraView);
        }
>>>>>>> Stashed changes:Assets/Script/Interact/FocusZone.cs
    }
}