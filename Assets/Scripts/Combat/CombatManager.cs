using UnityEngine;

namespace Combat
{
    public class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance;
        public Transform player;

        public static void SmallCharacterAttack(GameObject target)
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            
            if (damageable != null)
            {
                damageable.TakeDamage(50);
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
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
