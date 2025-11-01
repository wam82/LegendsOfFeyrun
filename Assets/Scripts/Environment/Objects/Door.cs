using System;
using System.Collections;
using Character;
using Environment.Interfaces;
using UnityEngine;

namespace Environment.Objects
{
    public class Door : MonoBehaviour, IInteractable
    {
        [SerializeField] private PlayableCharacter playableCharacter;
        [SerializeField] private float openingAngle;
        [SerializeField] private float duration;
        
        private bool _opened;
        private bool _isRotating;
        
        public bool CanInteract { get; set; }

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

        public void Interact()
        {
            if (CanInteract)
            {
                if (_isRotating)
                {
                    return;
                }

                StartCoroutine(Rotate());
                _opened = !_opened;
            }
            Debug.Log("Interacted with " + gameObject.name);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.gameObject.GetComponent<PlayableCharacter>().interactableObject =
                    gameObject.GetComponent<IInteractable>();
                CanInteract = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.gameObject.GetComponent<PlayableCharacter>().interactableObject = null;
                CanInteract = false;
            }
        }
    }
}