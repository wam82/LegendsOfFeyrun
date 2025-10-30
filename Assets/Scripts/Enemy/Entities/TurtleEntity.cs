using Combat;
using Enemy.Agents;
using UnityEngine;

namespace Enemy.Entities
{
    public class TurtleEntity : MonoBehaviour, IDamageable
    {
        [Header("Turtle Attributes")]
        [SerializeField] private float maxHealth;
        [SerializeField] private float walkSpeed;
        [SerializeField] private float runSpeed;
        [SerializeField] private float attackCooldown;
        [SerializeField] private float meleeAttackRadius;
        [SerializeField] private float rangedAttackRadius;
        
        public float WalkSpeed => walkSpeed;
        public float RunSpeed => runSpeed;
        public float AttackCooldown => attackCooldown;
        public float MeleeAttackRadius => meleeAttackRadius;
        public float RangedAttackRadius => rangedAttackRadius;
        
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
                    gameObject.GetComponent<TurtleAI>().Die();
                }
                else
                {
                    gameObject.GetComponent<TurtleAI>().GetHurt();
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