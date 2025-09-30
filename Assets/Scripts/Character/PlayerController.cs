using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Character
{
    public class PlayerController : MonoBehaviour
    {
        private static readonly int IsWalking = Animator.StringToHash("isWalking");
        private static readonly int IsSprinting = Animator.StringToHash("isSprinting");
        private static readonly int IsFalling = Animator.StringToHash("isFalling");
        private static readonly int IsShielding = Animator.StringToHash("isShielding");
        private PlayerInput _playerInput;
        private Character _character;
        private Animator _animator;
        
        public void OnMove(InputAction.CallbackContext context)
        {
            if (_character != null)
            {
                Vector2 movement = context.ReadValue<Vector2>();
                _character.SetInputVector(movement);
                _animator.SetBool(IsWalking, movement.sqrMagnitude > 0);
                
                if (!_animator.GetBool(IsWalking) && _character.SprintRequested)
                {
                    _character.RequestSprint();
                    _animator.SetBool(IsSprinting, _character.SprintRequested);
                }
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (_character != null && context.performed)
            {
                _character.RequestJump();
            }
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (_character != null && context.performed)
            {
                if (_animator.GetBool(IsWalking) && !_animator.GetBool(IsShielding))
                {
                    _character.RequestSprint();
                    _animator.SetBool(IsSprinting, _character.SprintRequested);
                }
            }
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Debug.Log("OnAttack");
            }
        }

        public void OnShield(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _animator.SetBool(IsShielding, true); 
                _character.RequestShield(_animator.GetBool(IsShielding));
                if (_animator.GetBool(IsSprinting)) 
                { 
                    _character.RequestSprint(); 
                    _animator.SetBool(IsSprinting, _character.SprintRequested);
                }
            }

            if (context.canceled)
            {
                _animator.SetBool(IsShielding, false);
                _character.RequestShield(_animator.GetBool(IsShielding));
            }
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Debug.Log("OnPause");
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                
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
            
            _animator = GetComponent<Animator>();
            if (_animator == null)
            {
                Debug.LogError("No animator found");
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

        private void Update()
        {
            if (!_character.IsGrounded)
            {
                _animator.SetBool(IsFalling, true);
            }
            else if (_character.IsGrounded && _animator.GetBool(IsFalling))
            {
                _animator.SetBool(IsFalling, false);
            }
        }
    }
}
