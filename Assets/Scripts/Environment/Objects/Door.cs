using System.Collections;
using Character;
using Combat;
using Environment.Interfaces;
using UnityEngine;
using UserInterface;

namespace Environment.Objects
{
    public class Door : MonoBehaviour, IInteractable
    {
        [SerializeField] private float openingAngle;
        [SerializeField] private float duration;
        
        private bool _opened;
        private bool _isRotating;
        private bool _isPlayerInTrigger;
        
        private PlayableCharacter _playableCharacter;

        private IEnumerator Rotate()
        {
            if (_isRotating)
            {
                yield break;
            }

            _isRotating = true;
            
            float elapsedTime = 0f;
            float startAngle = transform.eulerAngles.y;
            float targetAngle = startAngle + (_opened ? openingAngle * 1 : -openingAngle * 1);

            while (elapsedTime < duration)
            {
                float currentAngle = Mathf.Lerp(startAngle, targetAngle, elapsedTime / duration);
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, currentAngle, transform.eulerAngles.z);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.eulerAngles = new Vector3(transform.eulerAngles.x, targetAngle, transform.eulerAngles.z);
            _isRotating = false;
        }

        public bool CanInteract()
        {
            return !_isRotating && _isPlayerInTrigger;
        }

        public void Interact()
        {
            if (CanInteract())
            {
                StartCoroutine(Rotate());
                _opened = !_opened;
            }
            Debug.Log("Interacted with " + gameObject.name);
        }
        

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playableCharacter.interactableObject = this;
                _isPlayerInTrigger = true;
                
                UIManager.Instance.ShowInteractPrompt();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playableCharacter.interactableObject = null;
                _isPlayerInTrigger = false;
                
                UIManager.Instance.HideInteractPrompt();
            }
        }
        
        private void Start()
        {
            if (!_playableCharacter)
            {
                if (CombatManager.Instance.player)
                {
                    _playableCharacter = CombatManager.Instance.player.GetComponent<PlayableCharacter>();
                }
            }
        }
    }
}