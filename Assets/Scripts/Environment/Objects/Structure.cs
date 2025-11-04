using System;
using Character;
using UnityEngine;

namespace Environment.Objects
{
    public class Structure : MonoBehaviour
    {
        private PlayableCharacter _playableCharacter;
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playableCharacter = other.gameObject.GetComponent<PlayableCharacter>();
                if (!_playableCharacter.IsFPSCameraOn)
                {
                    _playableCharacter.ToggleActiveCamera();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playableCharacter = other.gameObject.GetComponent<PlayableCharacter>();
                if (_playableCharacter.IsFPSCameraOn)
                {
                    _playableCharacter.ToggleActiveCamera();
                }
            }
        }
    }
}