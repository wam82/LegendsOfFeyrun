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
        private PlayableCharacter _playableCharacter;
        private Animator _animator;

        private bool _canAttack;
        private float _lastAttackTime;
        private float _lastChargedAttackStart;
        private float _lastChargedAttackTime;
        
        public void OnMove(InputAction.CallbackContext context)
        {
            if (!_playableCharacter.IsDizzy && !_playableCharacter.IsDead)
            {
                Vector2 movement = context.ReadValue<Vector2>();
                _playableCharacter.RequestMove(movement);
                _playableCharacter.movementState = PlayableCharacter.MovementState.Walk;
                _animator.SetBool(IsWalking, movement.sqrMagnitude > 0);
                
                if (!_animator.GetBool(IsWalking) && _playableCharacter.SprintRequested)
                {
                    _playableCharacter.RequestSprint();
                    _animator.SetBool(IsSprinting, _playableCharacter.SprintRequested);
                }
            }

            if (_playableCharacter.IsDizzy)
            {
                _playableCharacter.RequestMove(new Vector2(0,0));
            }
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            if (_playableCharacter.IsFPSCameraOn)
            {
                _playableCharacter.RequestLook(context.ReadValue<Vector2>());
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed && !_playableCharacter.IsDizzy && !_playableCharacter.IsDead)
            {
                _playableCharacter.movementState = PlayableCharacter.MovementState.Jump;
                _playableCharacter.RequestJump();
            }
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.performed && !_playableCharacter.IsDizzy && !_playableCharacter.IsDead)
            {
                if (_animator.GetBool(IsWalking) && !_animator.GetBool(IsShielding))
                {
                    _playableCharacter.RequestSprint();
                    _playableCharacter.movementState = PlayableCharacter.MovementState.Sprint;
                    _animator.SetBool(IsSprinting, _playableCharacter.SprintRequested);
                }
            }
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.performed && !_animator.GetBool(IsAttacking) && _canAttack && !_animator.GetBool(IsShielding) && !_playableCharacter.IsDizzy && !_playableCharacter.IsDead)
            {
                if (_animator.GetBool(IsSprinting))
                {
                    _playableCharacter.RequestSprint();
                    _animator.SetBool(IsSprinting, _playableCharacter.SprintRequested);
                }
                _playableCharacter.RequestAttack();
                _playableCharacter.movementState = PlayableCharacter.MovementState.SmallAttack;
                _animator.SetBool(IsAttacking, _playableCharacter.IsAttacking);
            }
        }

        public void OnChargedAttack(InputAction.CallbackContext context)
        {
            if (context.performed && !_playableCharacter.IsDizzy && !_playableCharacter.IsDead)
            {
                _playableCharacter.IsChargedAttacking = true;
                _playableCharacter.movementState = PlayableCharacter.MovementState.ChargedAttack;
                _animator.SetBool(IsChargeAttacking, _playableCharacter.IsChargedAttacking);
                _lastChargedAttackStart = Time.time;
            }

            if (context.canceled)
            {
                _playableCharacter.IsChargedAttacking = false;
                _animator.SetBool(IsChargeAttacking, _playableCharacter.IsChargedAttacking);
            }
        }

        public void OnShield(InputAction.CallbackContext context)
        {
            if (context.performed && !_playableCharacter.IsDizzy && !_playableCharacter.IsDead)
            {
                _animator.SetBool(IsShielding, true);
                _playableCharacter.movementState = PlayableCharacter.MovementState.Shield;
                _playableCharacter.RequestShield(_animator.GetBool(IsShielding));
                if (_animator.GetBool(IsSprinting)) 
                { 
                    _playableCharacter.RequestSprint(); 
                    _animator.SetBool(IsSprinting, _playableCharacter.SprintRequested);
                }
            }

            if (context.canceled && !_playableCharacter.IsDizzy && !_playableCharacter.IsDead)
            {
                _animator.SetBool(IsShielding, false);
                _playableCharacter.RequestShield(_animator.GetBool(IsShielding));
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
                _playableCharacter.RequestInteract();
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
            
            _playableCharacter =  GetComponent<PlayableCharacter>();
            if (_playableCharacter == null)
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
            if (_playableCharacter.IsChargedAttacking)
            {
                if (Time.time - _lastChargedAttackStart > _playableCharacter.chargedAttackMaxDuration)
                {
                    _lastChargedAttackStart = 0;
                    _playableCharacter.IsChargedAttacking = false;
                    _animator.SetBool(IsChargeAttacking, _playableCharacter.IsChargedAttacking);
                    
                    _playableCharacter.IsDizzy = true;
                    _playableCharacter.movementState = PlayableCharacter.MovementState.Dizzy;
                    _animator.SetBool(IsDizzy, _playableCharacter.IsDizzy);
                    _lastChargedAttackTime = Time.time;
                    
                    _animator.SetBool(IsWalking, false);
                    _animator.SetBool(IsSprinting, false);
                    _animator.SetBool(IsFalling, false);
                    _animator.SetBool(IsShielding, false);
                }
            }

            if (_playableCharacter.IsDizzy)
            {
                if (Time.time - _lastChargedAttackTime > _playableCharacter.chargedAttackCooldown)
                {
                    _lastChargedAttackTime = 0;
                    _playableCharacter.IsDizzy = false;
                    _animator.SetBool(IsDizzy, _playableCharacter.IsDizzy);
                }
            }
            
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Attack01") && stateInfo.normalizedTime >= 1.0f && _animator.GetBool(IsAttacking))
            {
                // Debug.LogWarning("Attack01 Completed");
                _playableCharacter.IsAttacking = false;
                _animator.SetBool(IsAttacking, _playableCharacter.IsAttacking);
                _lastAttackTime =  Time.time;
                _canAttack = false;
            }
            else if (stateInfo.IsName("Attack02") && stateInfo.normalizedTime >= 1.0f && _animator.GetBool(IsAttacking))
            {
                // Debug.LogWarning("Attack02 Completed");
                _playableCharacter.IsAttacking = false;
                _animator.SetBool(IsAttacking, _playableCharacter.IsAttacking);
                _lastAttackTime =  Time.time;
                _canAttack = false;
            }
            else if (stateInfo.IsName("Attack03") && stateInfo.normalizedTime >= 1.0f && _animator.GetBool(IsAttacking))
            {
                // Debug.LogWarning("Attack03 Completed");
                _playableCharacter.IsAttacking = false;
                _animator.SetBool(IsAttacking, _playableCharacter.IsAttacking);
                _lastAttackTime =  Time.time;
                _canAttack = false;
            }

            if (Time.time - _lastAttackTime > _playableCharacter.attackCooldown)
            {
                _canAttack = true;
            }
            
            if (!_playableCharacter.IsGrounded)
            {
                _playableCharacter.movementState = PlayableCharacter.MovementState.Airborne;
                _animator.SetBool(IsFalling, true);
            }
            else if (_playableCharacter.IsGrounded && _animator.GetBool(IsFalling))
            {
                _animator.SetBool(IsFalling, false);
            }

            if (_playableCharacter.CurrentComboStep != _animator.GetInteger(AttackValue))
            {
                _animator.SetInteger(AttackValue, _playableCharacter.CurrentComboStep);
            }
        }
    }
}
