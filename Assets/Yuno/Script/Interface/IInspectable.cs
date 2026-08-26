using UnityEngine;

public interface IInspectable
{
    Transform ObjectTransform { get; }
    Vector3 InspectRotationOffset { get; }
    float InspectDistance { get; } // 🚨 추가: 카메라로부터의 거리
}