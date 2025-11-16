using System;
using System.Collections;
using System.Collections.Generic;
using Character;
using Combat;
using Environment.Interfaces;
using UnityEngine;

namespace Environment.Objects
{
    public class Gate : MonoBehaviour, IInteractable
    {
        [SerializeField] private KeyType requiredKeyType;
        [SerializeField] private float leftDoorOpeningAngle;
        [SerializeField] private float rightDoorOpeningAngle;
        [SerializeField] private float duration;
        [SerializeField] private GameObject leftDoor;
        [SerializeField] private GameObject rightDoor;
        
        
        private readonly Dictionary<GameObject, bool> _doorRotationState = new();
        private bool _opened;
        private bool _isPlayerInTrigger;
        
        private PlayableCharacter _playableCharacter;

        private IEnumerator RotateObject(GameObject targetObject,  float doorOpeningAngle)
        {
            if (_doorRotationState[targetObject] || !_doorRotationState.ContainsKey(targetObject))
            {
                yield break;
            }

            _doorRotationState[targetObject] = true;

            Transform target = targetObject.transform;

            float elapsedTime = 0f;
            float startAngle = target.eulerAngles.y;
            float targetAngle = startAngle + (_opened ? doorOpeningAngle : -doorOpeningAngle);

            while (elapsedTime < duration)
            {
                float currentAngle = Mathf.Lerp(startAngle, targetAngle, elapsedTime / duration);
                target.eulerAngles = new Vector3(
                    target.eulerAngles.x,
                    currentAngle,
                    target.eulerAngles.z
                );

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            target.eulerAngles = new Vector3(
                target.eulerAngles.x,
                targetAngle,
                target.eulerAngles.z
            );

            _doorRotationState[targetObject] = false;
        }

        public bool CanInteract()
        {
            return _isPlayerInTrigger && !_doorRotationState[leftDoor] &&  !_doorRotationState[rightDoor];
        }

        public void Interact()
        {
            if (CanInteract())
            {
                if (_playableCharacter.AttemptKeyTransaction(requiredKeyType))
                {
                    StartCoroutine(RotateObject(leftDoor, leftDoorOpeningAngle));
                    StartCoroutine(RotateObject(rightDoor, rightDoorOpeningAngle));
                    _opened = !_opened;
                }
                else
                {
                    Debug.Log("Interacted with " + gameObject.name + " but player had no keys of type " + requiredKeyType);
                }
            }
            
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playableCharacter.interactableObject = GetComponent<IInteractable>();
                _isPlayerInTrigger = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playableCharacter.interactableObject = null;
                _isPlayerInTrigger = false;
            }
        }

        private void Awake()
        {
            _doorRotationState[leftDoor] = false;
            _doorRotationState[rightDoor] = false;
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