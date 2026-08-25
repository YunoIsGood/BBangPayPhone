using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class InteractionStateManager : MonoBehaviour
{
    public static InteractionStateManager Instance { get; private set; }
    
    public GameState CurrentState { get; private set; } = GameState.FPS;
    public event Action<GameState> OnStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;
        
        CurrentState = newState;
        
        // 상태에 따른 마우스 잠금 통제
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