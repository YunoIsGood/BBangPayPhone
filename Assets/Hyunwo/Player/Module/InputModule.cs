using System;
using Script.Player.Input;
using UnityEngine;

namespace Script.Player.Module
{
    public class InputModule : MonoBehaviour
    {
        [SerializeField] private PlayerInputSO playerInput;
        
        [SerializeField] private CameraControlModule cameraControl;
        [SerializeField] private RaycastModule raycast;

        private void OnEnable()
        {
            playerInput.OnInteractPressed += OnHandleInteract;
            playerInput.OnCancelPressed += OnHandleCancel;
        }

        private void Update()
        {
            cameraControl.LookInput = playerInput.LookPos;
        }

        private void OnDisable()
        {            
            playerInput.OnInteractPressed -= OnHandleInteract;
            playerInput.OnCancelPressed -= OnHandleCancel;
        }

        private void OnHandleInteract()
        {
            raycast.OnInteractPerformed();
        }
        
        private void OnHandleCancel()
        {
            cameraControl.OnCancelPerformed();
        }        
    }
}