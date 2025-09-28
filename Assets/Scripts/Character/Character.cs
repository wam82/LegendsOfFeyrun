using System;
using UnityEngine;

namespace Character
{
    public class Character : MonoBehaviour
    {
        [Header("Character Attributes")]
        [SerializeField] private float movementSpeed = 5f;
        [SerializeField] private float rotationSpeed = 500f;
        [SerializeField] private float jumpForce = 5f;
        
        [Header("Character Components")]
        [SerializeField] private Transform cameraTransform;
        
        private Rigidbody _rigidbody;
        private Vector2 _inputVector;
        private Vector3 _movementDirection;
        private bool _jumpRequested;
        private bool _isGrounded;
        
        public void SetInputVector(Vector2 inputVector)
        {
            _inputVector = inputVector;
        }

        public void Jump()
        {
            if (_isGrounded)
            {
                _jumpRequested = true;
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
            Vector3 desiredDirection = camForward * _inputVector.y + camRight * _inputVector.x;
            
            // 3. Rotate character to face movement direction 
            if (desiredDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(desiredDirection);
                Quaternion newRotation = Quaternion.RotateTowards(_rigidbody.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
                _rigidbody.MoveRotation(newRotation);
            }
            
            // 4. Move using velocity (preserve vertical velocity for gravity/jump)
            Vector3 velocity = desiredDirection.normalized * movementSpeed;
            velocity.y = _rigidbody.velocity.y; 
            _rigidbody.velocity = velocity;
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

        private void FixedUpdate()
        {
            MoveCharacter();
        }
    }
}
