using System.Collections;
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
        private static readonly int IsChargeAttacking = Animator.StringToHash("isChargeAttacking");
        private static readonly int IsDizzy = Animator.StringToHash("isDizzy");
        private static readonly int IsDead =  Animator.StringToHash("isDead");
        private static readonly int IsSleeping = Animator.StringToHash("isSleeping");
        
        private PlayerInput _playerInput;
        private Character _character;
        private Animator _animator;

        private bool _canAttack;
        private float _lastAttackTime;
        private float _lastChargedAttackStart;
        private float _lastChargedAttackTime;
        
        public void OnMove(InputAction.CallbackContext context)
        {
            if (!_character.IsDizzy && !_character.IsDead)
            {
                Vector2 movement = context.ReadValue<Vector2>();
                _character.SetInputVector(movement);
                _character.movementState = Character.MovementState.Walk;
                _animator.SetBool(IsWalking, movement.sqrMagnitude > 0);
                
                if (!_animator.GetBool(IsWalking) && _character.SprintRequested)
                {
                    _character.RequestSprint();
                    _animator.SetBool(IsSprinting, _character.SprintRequested);
                }
            }

            if (_character.IsDizzy)
            {
                _character.SetInputVector(new Vector2(0,0));
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed && !_character.IsDizzy && !_character.IsDead)
            {
                _character.movementState = Character.MovementState.Jump;
                _character.RequestJump();
            }
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.performed && !_character.IsDizzy && !_character.IsDead)
            {
                if (_animator.GetBool(IsWalking) && !_animator.GetBool(IsShielding))
                {
                    _character.RequestSprint();
                    _character.movementState = Character.MovementState.Sprint;
                    _animator.SetBool(IsSprinting, _character.SprintRequested);
                }
            }
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.performed && !_animator.GetBool(IsAttacking) && _canAttack && !_animator.GetBool(IsShielding) && !_character.IsDizzy && !_character.IsDead)
            {
                if (_animator.GetBool(IsSprinting))
                {
                    _character.RequestSprint();
                    _animator.SetBool(IsSprinting, _character.SprintRequested);
                }
                _character.RequestAttack();
                _character.movementState = Character.MovementState.SmallAttack;
                _animator.SetBool(IsAttacking, _character.IsAttacking);
            }
        }

        public void OnChargedAttack(InputAction.CallbackContext context)
        {
            if (context.performed && !_character.IsDizzy && !_character.IsDead)
            {
                _character.IsChargedAttacking = true;
                _character.movementState = Character.MovementState.ChargedAttack;
                _animator.SetBool(IsChargeAttacking, _character.IsChargedAttacking);
                _lastChargedAttackStart = Time.time;
            }

            if (context.canceled)
            {
                _character.IsChargedAttacking = false;
                _animator.SetBool(IsChargeAttacking, _character.IsChargedAttacking);
            }
        }

        public void OnShield(InputAction.CallbackContext context)
        {
            if (context.performed && !_character.IsDizzy && !_character.IsDead)
            {
                _animator.SetBool(IsShielding, true);
                _character.movementState = Character.MovementState.Shield;
                _character.RequestShield(_animator.GetBool(IsShielding));
                if (_animator.GetBool(IsSprinting)) 
                { 
                    _character.RequestSprint(); 
                    _animator.SetBool(IsSprinting, _character.SprintRequested);
                }
            }

            if (context.canceled && !_character.IsDizzy && !_character.IsDead)
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

        private IEnumerator DelaySleep(float amount)
        {
            yield return new WaitForSeconds(amount);
            _animator.SetBool(IsSleeping,true);
        }
        public void Die()
        {
            _animator.SetBool(IsDead, true);
            StartCoroutine(DelaySleep(1));
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
            if (_character.IsChargedAttacking)
            {
                if (Time.time - _lastChargedAttackStart > _character.chargedAttackMaxDuration)
                {
                    _lastChargedAttackStart = 0;
                    _character.IsChargedAttacking = false;
                    _animator.SetBool(IsChargeAttacking, _character.IsChargedAttacking);
                    
                    _character.IsDizzy = true;
                    _character.movementState = Character.MovementState.Dizzy;
                    _animator.SetBool(IsDizzy, _character.IsDizzy);
                    _lastChargedAttackTime = Time.time;
                    
                    _animator.SetBool(IsWalking, false);
                    _animator.SetBool(IsSprinting, false);
                    _animator.SetBool(IsFalling, false);
                    _animator.SetBool(IsShielding, false);
                }
            }

            if (_character.IsDizzy)
            {
                if (Time.time - _lastChargedAttackTime > _character.chargedAttackCooldown)
                {
                    _lastChargedAttackTime = 0;
                    _character.IsDizzy = false;
                    _animator.SetBool(IsDizzy, _character.IsDizzy);
                }
            }
            
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Attack01") && stateInfo.normalizedTime >= 1.0f && _animator.GetBool(IsAttacking))
            {
                // Debug.LogWarning("Attack01 Completed");
                _character.IsAttacking = false;
                _animator.SetBool(IsAttacking, _character.IsAttacking);
                _lastAttackTime =  Time.time;
                _canAttack = false;
            }
            else if (stateInfo.IsName("Attack02") && stateInfo.normalizedTime >= 1.0f && _animator.GetBool(IsAttacking))
            {
                // Debug.LogWarning("Attack02 Completed");
                _character.IsAttacking = false;
                _animator.SetBool(IsAttacking, _character.IsAttacking);
                _lastAttackTime =  Time.time;
                _canAttack = false;
            }
            else if (stateInfo.IsName("Attack03") && stateInfo.normalizedTime >= 1.0f && _animator.GetBool(IsAttacking))
            {
                // Debug.LogWarning("Attack03 Completed");
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
                _character.movementState = Character.MovementState.Airborne;
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
