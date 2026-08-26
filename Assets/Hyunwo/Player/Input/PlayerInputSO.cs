using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Script.Player.Input
{
    [CreateAssetMenu(fileName = "PlayerInputSO", menuName = "SO/PlayerInput")]
    public class PlayerInputSO : ScriptableObject, Controls.IPlayerActions
    {
        private Controls _controls;
        
        public Vector2 LookPos { get; private set; }
        
        public event Action OnInteractPressed;
        public event Action OnCancelPressed;

        private void OnEnable()
        {
            if (_controls == null)
            {
                _controls = new Controls();
                _controls.Enable();
            }
            
            _controls.Player.SetCallbacks(this);
        }

        private void OnDisable()
        {
            _controls.Disable();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            if (context.performed)
                LookPos = context.ReadValue<Vector2>();
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnInteractPressed?.Invoke();
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnCancelPressed?.Invoke();
        }
    }
}
