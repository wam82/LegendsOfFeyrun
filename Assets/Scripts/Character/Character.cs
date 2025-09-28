using System;
using UnityEngine;

namespace Character
{
    public class Character : MonoBehaviour
    {
        [Header("Character Attributes")]
        [SerializeField] private float movementSpeed;
        [SerializeField] private float rotationSpeed;
        
        [Header("Character Components")]
        [SerializeField] private Transform cameraTransform;
        
        private CharacterController _characterController;
        
        private Vector2 _inputVector;
        
        public void SetInputVector(Vector2 inputVector)
        {
            _inputVector = inputVector;
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
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
            
            _characterController.Move(desiredDirection * (movementSpeed * Time.deltaTime));
        }

        private void Awake()
        {
            _characterController =  GetComponent<CharacterController>();
            if (_characterController == null)
            {
                Debug.LogError("Character controller not found");
            }
        }

        private void FixedUpdate()
        {
            MoveCharacter();
        }
    }
}
