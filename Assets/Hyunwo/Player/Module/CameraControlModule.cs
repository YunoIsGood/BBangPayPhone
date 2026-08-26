using PrimeTween;
using Script.Interact;
using UnityEngine;

namespace Script.Player.Module
{
    [DisallowMultipleComponent]
    public sealed class CameraControlModule : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float moveDuration = 0.5f;

        [Header("FPS Rotation Limits")]
        [SerializeField] private float minPitch = -45f;
        [SerializeField] private float maxPitch = 45f;
        [SerializeField] private float lookSensitivity = 0.5f;

        public Vector2 LookInput { get; set; }
        
        private float _xRotation;
        private float _yRotation;
    
        private Vector3 _fpsOriginPos;
        private Quaternion _fpsOriginRot;

        private Sequence _cameraSequence;
        
        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            _fpsOriginPos = cameraTransform.position;
            _fpsOriginRot = cameraTransform.rotation;
        
            SyncRotationVariables();    
        }

        public void DebugSystem()
        {
            Debug.Log("상호작용됨!");   
        }

        private void OnDisable()
        {
            if (_cameraSequence.isAlive) _cameraSequence.Stop();
        }

        private void LateUpdate()
        {
            if (InteractionStateManager.Instance.CurrentState != GameState.FPS || _cameraSequence.isAlive) return;

            if (LookInput.sqrMagnitude <= 0.01f || LookInput.sqrMagnitude > 10000f) return;

            _xRotation = Mathf.Clamp(_xRotation - (LookInput.y * lookSensitivity), minPitch, maxPitch);
            _yRotation += LookInput.x * lookSensitivity;

            cameraTransform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0f);
        }
        
        public void MoveToZone(Transform targetViewPoint)
        {
            if (_cameraSequence.isAlive) return;

            _fpsOriginPos = cameraTransform.position;
            _fpsOriginRot = cameraTransform.rotation;

            InteractionStateManager.Instance.ChangeState(GameState.Focused);
        
            _cameraSequence = Sequence.Create()
                .Group(Tween.Position(cameraTransform, targetViewPoint.position, moveDuration, Ease.InOutSine))
                .Group(Tween.Rotation(cameraTransform, targetViewPoint.rotation, moveDuration, Ease.InOutSine));
        }

        public void OnCancelPerformed()
        {
            // 🚨 핵심 수정: 카메라가 이동 중이거나, 물체가 날아가고 있는 중에는 우클릭 완전 무시! (더블 트리거 방지)
            if (_cameraSequence.isAlive) return;
            if (InspectViewer.Instance != null && InspectViewer.Instance.IsTransitioning) return;

            var currentState = InteractionStateManager.Instance.CurrentState;

            // 1. FocusZone(줌인) 상태에서 우클릭 -> FPS(기본 1인칭) 상태로 복귀
            if (currentState == GameState.Focused)
            {
                _cameraSequence = Sequence.Create()
                    .Group(Tween.Position(cameraTransform, _fpsOriginPos, moveDuration, Ease.InOutSine))
                    .Group(Tween.Rotation(cameraTransform, _fpsOriginRot, moveDuration, Ease.InOutSine))
                    .OnComplete(() => 
                    {
                        SyncRotationVariables();
                        InteractionStateManager.Instance.ChangeState(GameState.FPS);
                    });
            }
            // 2. Inspect(360도 관찰) 상태에서 우클릭 -> 뷰어에게 물건을 내려놓으라고 지시 (카메라는 움직이지 않음)
            else if (currentState == GameState.Inspect)
            {
                if (InspectViewer.Instance != null)
                {
                    InspectViewer.Instance.StopInspect();
                }
            }
        }

        private void SyncRotationVariables()
        {
            Vector3 angles = cameraTransform.eulerAngles;
            float normalizedX = angles.x > 180f ? angles.x - 360f : angles.x;
        
            _xRotation = Mathf.Clamp(normalizedX, minPitch, maxPitch);
            _yRotation = angles.y;
        }
    }
}