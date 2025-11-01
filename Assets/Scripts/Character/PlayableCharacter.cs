using System.Collections;
using Combat;
using Environment.Interfaces;
using UnityEngine;

namespace Character
{
    public class PlayableCharacter : MonoBehaviour, IDamageable
    {
        // #pragma warning disable CS0414 // Field is assigned but its value is never used
        public MovementState movementState;
        // #pragma warning restore CS0414 // Field is assigned but its value is never used

        [Header("Character Attributes")] 
        [SerializeField] private float maxHealth;
        [SerializeField] private float walkSpeed;
        [SerializeField] private float shieldSpeed;
        [SerializeField] private float sprintSpeed;
        [SerializeField] private float rotationSpeed = 500f;
        [SerializeField] private float jumpForce;
        [SerializeField] private float jumpCooldown;
        [SerializeField] private float airMultiplier;
        [SerializeField] private float groundDrag;
        [SerializeField] private float maxSlopeAngle;

        [Header("Combat Attributes")] 
        [SerializeField] private float comboTimer;
        [SerializeField] public float attackCooldown;
        [SerializeField] private float comboStep1Damage;
        [SerializeField] private float comboStep2Damage;
        [SerializeField] private float comboStep3Damage;
        [SerializeField] public float chargedAttackMaxDuration;
        [SerializeField] public float chargedAttackCooldown;
        [SerializeField] private float chargedAttackDamage;

        [Header("Character Components")] 
        [SerializeField] private float movementForceFactor = 10f;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private LayerMask groundLayerMask;
        [SerializeField] private PlayerController controller;

        public int CurrentComboStep { get; private set; }
        public float ChargedAttackDamage => chargedAttackDamage;

        public bool SprintRequested { get; private set; }
        public bool IsGrounded { get; private set; }
        public bool IsAttacking { get; set; }
        public bool IsChargedAttacking { get; set; }
        public bool IsDizzy { get; set; }
        public bool IsDead { get; private set; }
        
        private RaycastHit _slopeHit;

        public IInteractable interactableObject;
        
        private Rigidbody _rigidbody;
        
        private Vector2 _inputVector;
        private Vector3 _desiredDirection;
        
        private float _lastAttackTime;
        private float _movementSpeed;
        private float _currentHealth;
        private const float CharacterHeight = 1f;
        
        private bool _canJump = true;
        private bool _jumpRequested;
        private bool _shieldRequested;
        private bool _comboStarted;

        public enum MovementState
        {
            Walk,
            Sprint,
            Shield,
            SmallAttack,
            ChargedAttack,
            Jump,
            Dizzy,
            Airborne
        }

        public void SetInputVector(Vector2 inputVector)
        {
            _inputVector = inputVector;
        }

        private bool OnSlope()
        {
            if (Physics.Raycast(_rigidbody.position, Vector3.down, out _slopeHit, CharacterHeight * 0.5f + 0.3f))
            {
                float angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
                return angle <= maxSlopeAngle && angle != 0;
            }
            
            return false;
        }

        private Vector3 GetSlopeMoveDirection()
        {
            return Vector3.ProjectOnPlane(_desiredDirection, _slopeHit.normal).normalized;
        }

        public void RequestJump()
        {
            if (IsGrounded && _canJump)
            {
                _jumpRequested = true;
            }
        }

        public void RequestSprint()
        {
            SprintRequested = !SprintRequested;
            
        }

        public void RequestShield(bool shieldRequested)
        {
            _shieldRequested = shieldRequested;
        }

        public void RequestAttack()
        {
            if (!IsAttacking)
            {
                CurrentComboStep++;
                
                // Debug.Log("Combo Step: " + CurrentComboStep);

                _comboStarted = true;
                IsAttacking = true;
                _lastAttackTime = Time.time;
                
                if (CurrentComboStep > 3)
                {
                    CurrentComboStep = 1;
                }
            }
        }

        public void RequestInteract()
        {
            interactableObject?.Interact();
        }

        public float GetSmallAttackDamage()
        {
            float damage = 0;
            switch (CurrentComboStep)
            {
                case 1:
                    damage = comboStep1Damage;
                    break;
                case 2:
                    damage = comboStep2Damage;
                    break;
                case 3:
                    damage = comboStep3Damage;
                    break;
                default:
                    damage = 0;
                    break;
            }
            return damage;
        }

        private void CheckIsGrounded()
        {
            IsGrounded = Physics.Raycast(transform.position, Vector3.down,  CharacterHeight * 0.5f + 0.3f,  groundLayerMask);
        }

        private void Jump()
        {
            // Reset Y velocity so jump is consistent
            _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);
            
            _rigidbody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
        
        private IEnumerator ResetJumpCoroutine()
        {
            yield return new WaitForSeconds(jumpCooldown);
            _canJump = true;
        }
        
        private void SpeedHandler()
        {
            if (IsGrounded && SprintRequested)
            {
                // _movementState = MovementState.Sprint;
                _movementSpeed = sprintSpeed;
            }
            else if (IsGrounded && _shieldRequested)
            {
                // _movementState = MovementState.Shield;
                _movementSpeed = shieldSpeed;
            }
            else if (IsGrounded && IsAttacking)
            {
                _movementSpeed = 0f;
            }
            else if (IsGrounded)
            {
                // _movementState = MovementState.Walk;
                _movementSpeed = walkSpeed;
            }
            else
            {
                // _movementState =  MovementState.Airborne;
            }
        }
        
        private void MoveCharacter()
        {
            // 1. Get camera's forward and right, ignoring vertical component
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();
            
            // 2. Compute movement direction from input
            _desiredDirection = camForward * _inputVector.y + camRight * _inputVector.x;
            
            // 3. Rotate character to face movement direction 
            if (_desiredDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(_desiredDirection);
                Quaternion newRotation = Quaternion.RotateTowards(_rigidbody.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
                _rigidbody.MoveRotation(newRotation);
            }
            
            // 4. Apply movement on the character's rigid body
            if (_desiredDirection.sqrMagnitude > 0.01f)
            {
                if (OnSlope())
                {
                    _rigidbody.AddForce(GetSlopeMoveDirection() * (_movementSpeed * 0.5f * movementForceFactor), ForceMode.Force);
                }
                else if (IsGrounded)
                {
                    _rigidbody.AddForce(_desiredDirection.normalized * (_movementSpeed * movementForceFactor), ForceMode.Force);
                }
                else if (!IsGrounded)
                {
                    _rigidbody.AddForce(_desiredDirection.normalized * (_movementSpeed * movementForceFactor * airMultiplier), ForceMode.Force);
                }
            }
            
            // 5. Clamp horizontal velocity
            Vector3 flatVelocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);
            if (flatVelocity.magnitude > _movementSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * _movementSpeed;
                _rigidbody.velocity = new Vector3(limitedVelocity.x, _rigidbody.velocity.y, limitedVelocity.z);
            }
        }

        private void Awake()
        {
            _rigidbody =  GetComponent<Rigidbody>();
            if (_rigidbody == null)
            {
                Debug.LogError("Rigid body not found");
            }
            _rigidbody.freezeRotation = true;
            
            _currentHealth = maxHealth;
        }

        private void Update()
        {
            if (_comboStarted && (Time.time - _lastAttackTime > comboTimer || CurrentComboStep == 3))
            {
                CurrentComboStep = 0;
                _comboStarted = false;
                // Debug.LogWarning("Reset combo");
            }
            
            CheckIsGrounded();

            if (IsDizzy)
            {
                return;
            }
            
            if (IsGrounded)
            {
                _rigidbody.drag = groundDrag;
            }
            else
            {
                _rigidbody.drag = 0;
            }

            if (_jumpRequested)
            {
                _canJump  = false;
                _jumpRequested = false;
                
                Jump();
                
                StartCoroutine(ResetJumpCoroutine());
            }
            
            SpeedHandler();
        }

        private void FixedUpdate()
        {
            if (IsDizzy || IsDead)
            {
                return;
            }
            
            MoveCharacter();
        }

        public void TakeDamage(float amount)
        {
            if (_shieldRequested)
            {
                if (Random.value < 0.5f)
                {
                    amount /= 2;
                }
            }
            
            _currentHealth -= amount;

            if (_currentHealth <= 0)
            {
                IsDead = true;
                controller.Die();
            }
        }
    }
}
