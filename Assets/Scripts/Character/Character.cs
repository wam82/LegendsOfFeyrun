using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Character
{
    public class Character : MonoBehaviour
    {
        [Header("Character Attributes")]
        [SerializeField] private float walkSpeed;
        [SerializeField] private float sprintSpeed;
        [SerializeField] private float rotationSpeed = 500f;
        [SerializeField] private float jumpForce;
        [SerializeField] private float jumpCooldown;
        [SerializeField] private float airMultiplier;
        [SerializeField] private float groundDrag;
        [SerializeField] private float maxSlopeAngle;

        [Header("Character Components")] 
        [SerializeField] private float movementForceFactor = 10f;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private LayerMask groundLayerMask;
        
        public enum MovementState
        {
            Walk,
            Sprint,
            Airborne
        }
        
#pragma warning disable CS0414 // Field is assigned but its value is never used
        private MovementState _movementState;
#pragma warning restore CS0414 // Field is assigned but its value is never used
        private RaycastHit _slopeHit;
        private float _movementSpeed;
        private Rigidbody _rigidbody;
        private Vector2 _inputVector;
        private Vector3 _desiredDirection;
        private bool _isGrounded;
        private bool _canJump = true;
        private bool _jumpRequested;
        private bool _sprintRequested;
        private const float CharacterHeight = 1f;

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
            if (_isGrounded && _canJump)
            {
                _jumpRequested = true;
            }
        }

        public void RequestSprint()
        {
            _sprintRequested = !_sprintRequested;
        }

        private void IsGrounded()
        {
            _isGrounded = Physics.Raycast(transform.position, Vector3.down,  CharacterHeight * 0.5f + 0.3f,  groundLayerMask);
        }
        
        private void StateHandler()
        {
            if (_isGrounded && _sprintRequested)
            {
                _movementState = MovementState.Sprint;
                _movementSpeed = sprintSpeed;
            }
            else if (_isGrounded)
            {
                _movementState = MovementState.Walk;
                _movementSpeed = walkSpeed;
            }
            else
            {
                _movementState = MovementState.Airborne;
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
                else if (_isGrounded)
                {
                    _rigidbody.AddForce(_desiredDirection.normalized * (_movementSpeed * movementForceFactor), ForceMode.Force);
                }
                else if (!_isGrounded)
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

        private void Jump()
        {
            // Reset Y velocity so jump is consistent
            _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);
            
            _rigidbody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
        
        private IEnumerator ResetJumpCoroutine()
        {
            yield return new WaitForSeconds(jumpCooldown);
            ResetJump();
        }

        private void ResetJump()
        {
            _canJump = true;
        }

        private void Awake()
        {
            _rigidbody =  GetComponent<Rigidbody>();
            if (_rigidbody == null)
            {
                Debug.LogError("Rigid body not found");
            }
            _rigidbody.freezeRotation = true;
        }

        private void Update()
        {
            IsGrounded();

            if (_isGrounded)
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
            
            StateHandler();
        }

        private void FixedUpdate()
        {
            MoveCharacter();
        }
    }
}
