using UnityEngine;

namespace Combat
{
    public class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance;
        public Transform player;

        public static void CharacterAttack(GameObject target, float damage)
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
            else
            {
                Debug.LogWarning($"Target {target.name} cannot be damaged (no IDamageable component).");
            }
        }

        public static void SlimeAttack(GameObject target, float damage)
        {
            IDamageable damageable = target.GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                Debug.Log($"Target {target.name} took {damage} damage.");
            }
            else
            {
                Debug.LogWarning($"Target {target.name} cannot be damaged (no IDamageable component).");
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
