using UnityEngine;

public interface IInspectable : IInteractable
{
    Vector3 InspectRotationOffset { get; }
    Transform ObjectTransform { get; }
}