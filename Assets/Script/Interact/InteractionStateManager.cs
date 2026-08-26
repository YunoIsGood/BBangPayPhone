using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class InteractionStateManager : MonoBehaviour
{
    public static InteractionStateManager Instance { get; private set; }
    
    public GameState CurrentState { get; private set; } = GameState.FPS;
    
    // 🚨 추가: 현재 줌인(Focus)되어 있는 구역의 부모 Transform 추적
    public Transform CurrentFocusZone { get; private set; } 
    
    public event Action<GameState> OnStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    // 구역 설정 함수
    public void SetFocusZone(Transform zoneTransform)
    {
        CurrentFocusZone = zoneTransform;
    }

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;
        
        CurrentState = newState;

        // FPS 상태(기본 시점)로 돌아오면 FocusZone 기억을 비움
        if (newState == GameState.FPS)
        {
            CurrentFocusZone = null;
        }
        
        if (newState == GameState.FPS)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        OnStateChanged?.Invoke(CurrentState);
    }
}