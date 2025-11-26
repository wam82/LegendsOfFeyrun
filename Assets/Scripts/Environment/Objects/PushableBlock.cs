using System;
using System.Collections;
using Character;
using Environment.Interfaces;
using UnityEngine;
using UserInterface;

namespace Environment.Objects
{
    public class PushableBlock : MonoBehaviour, IInteractable
    {
        [SerializeField] private float moveDuration;
     
        private bool _isMoving;
        private bool _isPlayerPushing;

        public bool CanInteract()
        {
            return !_isMoving && _isPlayerPushing;
        }

        public void Interact()
        {
            if (CanInteract())
            {
                float blockSize = transform.localScale.z;
                Vector3 targetPosition = transform.position + transform.forward * blockSize * 2;
                StartCoroutine(MoveBlock(targetPosition));
            }   
        }
        
        private IEnumerator MoveBlock(Vector3 targetPosition)
        {
            _isMoving = true;
            
            Vector3 startPosition = transform.position;
            
            bool hasObstacle = Physics.Raycast(
                startPosition,
                transform.forward,
                out RaycastHit hit,
                1f
            );

            Vector3 actualTarget = targetPosition;
            
            if (hasObstacle)
            {
                actualTarget = startPosition + transform.forward * 1f;
            }
            
            float elapsedTime = 0f;

            while (elapsedTime < moveDuration)
            {
                transform.position = Vector3.Lerp(startPosition, actualTarget, elapsedTime / moveDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.position = actualTarget;
            
            if (hasObstacle)
            {
                elapsedTime = 0f;
                while (elapsedTime < moveDuration)
                {
                    transform.position = Vector3.Lerp(actualTarget, startPosition, elapsedTime / moveDuration);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                transform.position = startPosition;
            }
            
            _isMoving = false;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                Vector3 playerForward = other.transform.forward;
                Vector3 blockForward = transform.forward;
                
                float alignment = Vector3.Dot(playerForward.normalized, blockForward.normalized);

                if (alignment > 0.9f)
                {
                    other.gameObject.GetComponent<PlayableCharacter>().interactableObject =
                        gameObject.GetComponent<IInteractable>();
                    _isPlayerPushing = true;
                    
                    UIManager.Instance.ShowInteractPrompt();
                }
            }
        }

        private void OnCollisionExit(Collision other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                other.gameObject.GetComponent<PlayableCharacter>().interactableObject = null;
                _isPlayerPushing = false;
                
                UIManager.Instance.HideInteractPrompt();
            }
        }
    }
}