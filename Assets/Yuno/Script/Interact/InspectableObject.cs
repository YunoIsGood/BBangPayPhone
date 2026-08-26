using UnityEngine;
<<<<<<< Updated upstream:Assets/Yuno/Script/Interact/InspectableObject.cs
using PrimeTween;
using Script.Interact;
=======
>>>>>>> Stashed changes:Assets/Script/Interact/InspectableObject.cs

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public sealed class InspectableObject : MonoBehaviour, IInspectable
{
    [field: Header("Inspect Settings")]
    [field: SerializeField, Tooltip("관찰 시 모델링 방향 보정 값")]
    public Vector3 InspectRotationOffset { get; private set; } = Vector3.zero;

    [field: SerializeField, Tooltip("카메라와 아이템 사이의 거리 (기본값: 0.45m / 작은 물건: 0.35m)")]
    public float InspectDistance { get; private set; } = 0.45f; // 🚨 추가

    public Transform ObjectTransform => transform;
    public Vector3 OriginalPos { get; private set; }
    public Quaternion OriginalRot { get; private set; }
    public Collider MainCollider { get; private set; }

    private void Awake()
    {
        MainCollider = GetComponent<Collider>();
        OriginalPos = transform.position;
        OriginalRot = transform.rotation;
    }

    public void ResetObject()
    {
        transform.position = OriginalPos;
        transform.rotation = OriginalRot;
    }
}