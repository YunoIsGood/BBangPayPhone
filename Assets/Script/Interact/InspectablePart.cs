using UnityEngine;
using UnityEngine.Events;

public sealed class InspectablePart : MonoBehaviour, IInteractable
{
    [SerializeField] private UnityEvent onInteractEvent;

    // 줌인 상태이거나, 360도 관찰 중일 때만 클릭 가능
    public bool CanInteract => 
        InteractionStateManager.Instance.CurrentState == GameState.Focused || 
        InteractionStateManager.Instance.CurrentState == GameState.Inspect;

    public void Interact()
    {
        if (CanInteract) onInteractEvent?.Invoke();
    }
}