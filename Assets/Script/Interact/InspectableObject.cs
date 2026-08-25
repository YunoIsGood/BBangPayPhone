using UnityEngine;
using PrimeTween;
using Script.Interact;

public sealed class InspectableObject : MonoBehaviour, IInspectable
{
    [field: SerializeField] public Vector3 InspectRotationOffset { get; private set; }
    public Transform ObjectTransform => transform;
    
    // Focused(줌인) 상태에서만 물건을 집어들 수 있음
    public bool CanInteract => InteractionStateManager.Instance.CurrentState == GameState.Focused;

    public void Interact() => InspectViewer.Instance.StartInspect(this);
}