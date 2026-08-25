using UnityEngine;
using PrimeTween;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public sealed class InspectableObject : MonoBehaviour, IInteractable
{
    [field: Header("Inspect Settings")]
    [field: SerializeField, Tooltip("관찰 시 모델링 방향 보정 값")]
    public Vector3 InspectRotationOffset { get; private set; } = Vector3.zero;

    public Vector3 OriginalPos { get; private set; }
    public Quaternion OriginalRot { get; private set; }
    public Collider MainCollider { get; private set; }
    
    public bool CanInteract => !InspectManager.IsInspecting;

    private void Awake()
    {
        MainCollider = GetComponent<Collider>();
        OriginalPos = transform.position;
        OriginalRot = transform.rotation;
    }

    public void Interact()
    {
        if (!CanInteract) return;
        InspectManager.Instance?.StartInspect(this);
    }

    public void ResetObject(float duration = 0.3f)
    {
        Tween.Position(transform, OriginalPos, duration, Ease.OutQuad);
        Tween.Rotation(transform, OriginalRot, duration, Ease.OutQuad);
    }
}