using System;
using Character;
using Combat;
using Environment.Interfaces;
using UnityEngine;

namespace Environment.Objects
{
    public class Food : MonoBehaviour, ICollectible
    {
        [SerializeField] private float restoreAmount;
        private PlayableCharacter _playableCharacter;
        public bool Collected { get; set; }

        public void Collect()
        {
            Collected = true;
            _playableCharacter.RestoreHealth(restoreAmount);
        }

        private void Start()
        {
            if (!_playableCharacter)
            {
                if (CombatManager.Instance.player != null)
                {
                    _playableCharacter = CombatManager.Instance.player.GetComponent<PlayableCharacter>();
                }
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (!Collected)
                {
                    Collect();
                }
            }
        }
    }
}