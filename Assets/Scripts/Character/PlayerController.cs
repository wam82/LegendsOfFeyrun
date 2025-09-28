using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Character
{
    public class PlayerController : MonoBehaviour
    {
        private PlayerInput _playerInput;
        private Character _character;
        
        public void OnMove(InputAction.CallbackContext context)
        {
            // Debug.Log("OnMove");
            if (_character != null)
            {
                _character.SetInputVector(context.ReadValue<Vector2>());
            }
        }

        private void Awake()
        {
            _playerInput =  GetComponent<PlayerInput>();
            if (_playerInput == null)
            {
                Debug.LogError("No player input found");
            }
            
            _character =  GetComponent<Character>();
            if (_character == null)
            {
                Debug.LogError("No character found");
            }
        }

        private void Start()
        {
            InputActionMap map = _playerInput.actions.FindActionMap("Game");

            // Enable the action map
            if (map != null)
            {
                map.Enable();
            }
            else
            {
                Debug.LogError("Action Map 'Game' not found!");
            }
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
