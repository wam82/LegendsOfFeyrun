using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Character
{
    public class Character : MonoBehaviour
    {
        [Header("Character Attributes")]
        [SerializeField] private float movementSpeed = 5f;
        [SerializeField] private float rotationSpeed = 500f;
        [SerializeField] private float jumpForce;
        [SerializeField] private float jumpCooldown;
        [SerializeField] private float airMultiplier;
        [SerializeField] private float groundDrag;
        // [SerializeField] private float gravity = 9.8f;

        [Header("Character Components")] 
        [SerializeField] private float movementForceFactor = 10f;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private LayerMask groundLayerMask;
        
        private Rigidbody _rigidbody;
        private Vector2 _inputVector;
        private Vector3 _movementDirection;
        private bool _isGrounded;
        private bool _canJump = true;
        private bool _jumpRequested;
        private readonly float _characterHeight = 1f;
        
        public void SetInputVector(Vector2 inputVector)
        {
            _inputVector = inputVector;
        }

        public void RequestJump()
        {
            if (_isGrounded && _canJump)
            {
                // Debug.Log("Jump");
                _jumpRequested = true;
            }
        }

        private void IsGrounded()
        {
            _isGrounded = Physics.Raycast(transform.position, Vector3.down,  _characterHeight * 0.5f + 0.2f,  groundLayerMask);
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
            Vector3 desiredDirection = camForward * _inputVector.y + camRight * _inputVector.x;
            
            // 3. Rotate character to face movement direction 
            if (desiredDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(desiredDirection);
                Quaternion newRotation = Quaternion.RotateTowards(_rigidbody.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
                _rigidbody.MoveRotation(newRotation);
            }
            
            // 4. Apply movement on the character's rigid body
            if (desiredDirection.sqrMagnitude > 0.01f)
            {
                if (_isGrounded)
                {
                    _rigidbody.AddForce(desiredDirection.normalized * (movementSpeed * movementForceFactor), ForceMode.Force);
                }
                else
                {
                    _rigidbody.AddForce(desiredDirection.normalized * (movementSpeed * movementForceFactor * airMultiplier), ForceMode.Force);
                }
            }
            
            // 5. Clamp horizontal velocity
            Vector3 flatVelocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);
            if (flatVelocity.magnitude > movementSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * movementSpeed;
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
        }

        private void FixedUpdate()
        {
            MoveCharacter();
        }
    }
}
