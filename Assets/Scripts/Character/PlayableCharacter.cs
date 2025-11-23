using System;
using System.Collections;
using System.Collections.Generic;
using Combat;
using Environment.Interfaces;
using Environment.Objects;
using UnityEngine;
using UserInterface;
using Random = UnityEngine.Random;

namespace Character
{
    public class PlayableCharacter : MonoBehaviour, IDamageable
    {
        [ContextMenu("Take 25 Damage")]
        private void Take25Damage()
        {
            TakeDamage(25f);
        }
        
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
        
        [Header("Player Settings")]
        [SerializeField] private float horizontalSensitivity;
        [SerializeField] private float verticalSensitivity;

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
        [SerializeField] private Transform firstPersonCamera;
        [SerializeField] private Transform thirdPersonCamera;
        [SerializeField] private LayerMask groundLayerMask;
        [SerializeField] private PlayerController controller;

        public int CurrentComboStep { get; private set; }
        public float ChargedAttackDamage => chargedAttackDamage;
        public float Speed => _movementSpeed;

        public bool IsFPSCameraOn { get; set; }
        public bool SprintRequested { get; private set; }
        public bool IsGrounded { get; private set; }
        public bool IsAttacking { get; set; }
        public bool IsChargedAttacking { get; set; }
        public bool IsDizzy { get; set; }
        public bool IsDead { get; private set; }
        
        private RaycastHit _slopeHit;

        public IInteractable interactableObject;
        
        private Rigidbody _rigidbody;
        
        private Vector2 _movementInputVector;
        private Vector2 _lookInputVector;
        private Vector3 _desiredDirection;
        
        private float _lastAttackTime;
        private float _movementSpeed;
        private float _currentHealth;
        private const float CharacterHeight = 1f;
        private float _xRotation = 0f;
        
        private bool _canJump = true;
        private bool _jumpRequested;
        private bool _shieldRequested;
        private bool _comboStarted;

        private readonly Dictionary<KeyType, List<Key>> _collectedKeys = new();


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

        public void AddKey(Key key)
        {
            KeyType type = key.type;

            if (!_collectedKeys.TryGetValue(type, out var list))
            {
                list = new List<Key>();
                _collectedKeys[type] = list;
            }

            list.Add(key);
        }

        public bool AttemptKeyTransaction(KeyType type)
        {
            if (_collectedKeys.ContainsKey(type))
            {
                if (_collectedKeys.TryGetValue(type, out var list))
                {
                    if (list.Count > 0)
                    {
                        // Take the first key
                        Key firstKey = list[0];

                        // Remove the key
                        list.Remove(firstKey);
                        firstKey.gameObject.SetActive(false);

                        // If the list is now empty, remove the entire entry from the dictionary
                        if (list.Count == 0)
                        {
                            _collectedKeys.Remove(type);
                        }
                
                        return true;
                    }

                    Debug.LogWarning("List is empty");
                }
                else
                {
                    Debug.LogWarning("Couldn't pull value from dictionary");
                }
            }
            else
            {
                Debug.LogWarning("No key type found for " + type);
            }
            
            return false;
        }

        public void RequestMove(Vector2 inputVector)
        {
            _movementInputVector = inputVector;
        }

        public void RequestLook(Vector2 inputVector)
        {
            _lookInputVector = inputVector;
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

        public void ToggleActiveCamera()
        {
            if (IsFPSCameraOn)
            {
                firstPersonCamera.gameObject.SetActive(false);
                thirdPersonCamera.gameObject.SetActive(true);
                IsFPSCameraOn = false;
            }
            else
            {
                firstPersonCamera.gameObject.SetActive(true);
                thirdPersonCamera.gameObject.SetActive(false);
                IsFPSCameraOn = true;
            }
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

        private void FirstPersonLookAround()
        {
            float mouseX = _lookInputVector.x * horizontalSensitivity * Time.deltaTime;
            float mouseY = _lookInputVector.y * verticalSensitivity * Time.deltaTime;
            
            _xRotation -= mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -80f, 80f);

            firstPersonCamera.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);
        }

        private void FirstPersonMovement()
        {
            // 1. Get camera forward and right, ignoring vertical component (so you only move horizontally)
            Vector3 camForward = firstPersonCamera.forward;
            Vector3 camRight = firstPersonCamera.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            // 2. Compute movement direction from input (relative to camera orientation)
            _desiredDirection = camForward * _movementInputVector.y + camRight * _movementInputVector.x;

            // 3. Apply movement forces
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
                else // In air
                {
                    _rigidbody.AddForce(_desiredDirection.normalized * (_movementSpeed * movementForceFactor * airMultiplier), ForceMode.Force);
                }
            }

            // 4. Clamp horizontal velocity
            Vector3 flatVelocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);
            if (flatVelocity.magnitude > _movementSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * _movementSpeed;
                _rigidbody.velocity = new Vector3(limitedVelocity.x, _rigidbody.velocity.y, limitedVelocity.z);
            }
        }
        
        private void ThirdPersonMovement()
        {
            // 1. Get camera's forward and right, ignoring vertical component
            Vector3 camForward = thirdPersonCamera.forward;
            Vector3 camRight = thirdPersonCamera.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();
            
            // 2. Compute movement direction from input
            _desiredDirection = camForward * _movementInputVector.y + camRight * _movementInputVector.x;
            
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

        private void Start()
        {
            UIManager.Instance.InstantiateHealthBar(maxHealth);
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

            if (IsFPSCameraOn)
            {
                FirstPersonMovement();
                FirstPersonLookAround();
            }
            else
            {
                ThirdPersonMovement();
            }
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
            _currentHealth = Mathf.Max(0,  _currentHealth);
            UIManager.Instance.SetHealth(_currentHealth);

            if (_currentHealth <= 0)
            {
                IsDead = true;
                controller.Die();
            }
        }
        
        public void RestoreHealth(float amount)
        {
            _currentHealth += amount;
            _currentHealth = Mathf.Min(_currentHealth, maxHealth);
            UIManager.Instance.SetHealth(_currentHealth);
        }
    }
}
