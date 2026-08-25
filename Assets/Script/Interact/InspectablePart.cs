using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public sealed class InspectablePart : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private UnityEvent onInteractEvent;

    public bool CanInteract => InspectManager.IsInspecting;

    public void Interact()
    {
        if (!CanInteract) return;
        onInteractEvent?.Invoke(); 
    }
}