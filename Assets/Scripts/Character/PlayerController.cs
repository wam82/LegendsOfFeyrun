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
        private static readonly int AttackValue = Animator.StringToHash("attackValue");
        private static readonly int IsAttacking = Animator.StringToHash("isAttacking");
        private PlayerInput _playerInput;
        private Character _character;
        private Animator _animator;

        private bool _canAttack;
        private float _lastAttackTime;
        
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
            // Insert attack cooldown so you can void all inputs while animations plays and completes.
            // Currently, spamming causes increments during animation transitions.
            // As such, by spamming, you can get from stage 1 to stage 3 directly, skipping step 2.
            if (context.performed && !_animator.GetBool(IsAttacking) && _canAttack)
            {
                _character.RequestAttack();
                _animator.SetBool(IsAttacking, _character.IsAttacking);
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
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Attack01") && stateInfo.normalizedTime >= 1.0f && _animator.GetBool(IsAttacking))
            {
                Debug.LogWarning("Attack01 Completed");
                _character.IsAttacking = false;
                _animator.SetBool(IsAttacking, _character.IsAttacking);
                _lastAttackTime =  Time.time;
                _canAttack = false;
            }
            else if (stateInfo.IsName("Attack02") && stateInfo.normalizedTime >= 1.0f && _animator.GetBool(IsAttacking))
            {
                Debug.LogWarning("Attack02 Completed");
                _character.IsAttacking = false;
                _animator.SetBool(IsAttacking, _character.IsAttacking);
                _lastAttackTime =  Time.time;
                _canAttack = false;
            }
            else if (stateInfo.IsName("Attack03") && stateInfo.normalizedTime >= 1.0f && _animator.GetBool(IsAttacking))
            {
                Debug.LogWarning("Attack03 Completed");
                _character.IsAttacking = false;
                _animator.SetBool(IsAttacking, _character.IsAttacking);
                _lastAttackTime =  Time.time;
                _canAttack = false;
            }

            if (Time.time - _lastAttackTime > _character.attackCooldown)
            {
                _canAttack = true;
            }
            
            if (!_character.IsGrounded)
            {
                _animator.SetBool(IsFalling, true);
            }
            else if (_character.IsGrounded && _animator.GetBool(IsFalling))
            {
                _animator.SetBool(IsFalling, false);
            }

            if (_character.CurrentComboStep != _animator.GetInteger(AttackValue))
            {
                _animator.SetInteger(AttackValue, _character.CurrentComboStep);
            }
        }
    }
}
