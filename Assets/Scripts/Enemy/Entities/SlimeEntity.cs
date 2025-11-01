using Combat;
using Enemy.Agents;
using UnityEngine;

namespace Enemy.Entities
{
    public class SlimeEntity : MonoBehaviour, IDamageable
    {
        [Header("Slime Attributes")]
        [SerializeField] private float maxHealth;
        [SerializeField] private float walkSpeed;
        [SerializeField] private float sneakSpeed;
        [SerializeField] private float runSpeed;
        [SerializeField] private float detectionRadius;
        [SerializeField] private float attack1Radius;
        [SerializeField] private float attack2Radius;
        [SerializeField] private float attackCooldown;
        [SerializeField] private float attack1Damage;
        [SerializeField] private float attack2Damage;
        
        // Public Getters
        public float WalkSpeed => walkSpeed;
        public float SneakSpeed => sneakSpeed;
        public float RunSpeed => runSpeed;
        public float DetectionRadius => detectionRadius;
        public float Attack1Radius => attack1Radius;
        public float Attack2Radius => attack2Radius;
        public float AttackCooldown => attackCooldown;
        public float Attack1Damage => attack1Damage;
        public float Attack2Damage => attack2Damage;


        private float _currentHealth;
        private bool _wasTriggered;
        
        public void TakeDamage(float amount)
        {
            if (!_wasTriggered)
            {
                _wasTriggered = true;
                _currentHealth -= amount;

                if (_currentHealth <= 0)
                {
                    gameObject.GetComponent<SlimeAI>().Die();
                }
                else
                {
                    gameObject.GetComponent<SlimeAI>().GetHurt();
                }
                _wasTriggered = false;
            }
        }
        
        private void Awake()
        {
            _currentHealth = maxHealth;
        }
    }
}