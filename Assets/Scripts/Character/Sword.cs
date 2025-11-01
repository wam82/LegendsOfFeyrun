using Combat;
using UnityEngine;

namespace Character
{
    public class Sword : MonoBehaviour
    {
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private PlayableCharacter playableCharacter;
        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & enemyLayer) != 0)
            {
                if (playableCharacter.IsAttacking)
                {
                    CombatManager.CharacterAttack(other.gameObject, playableCharacter.GetSmallAttackDamage());
                    // Debug.Log("Hit enemy: " + other.name);   
                }

                if (playableCharacter.IsChargedAttacking)
                {
                    CombatManager.CharacterAttack(other.gameObject, playableCharacter.ChargedAttackDamage);
                }
            }
        }

        // private void OnTriggerStay(Collider other)
        // {
        //     if (character.IsAttacking || character.IsChargeAttacking)
        //     {
        //         if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        //         {
        //             CombatManager.Instance.Attack(other.gameObject);
        //         }
        //     }
        // }
    }
}
