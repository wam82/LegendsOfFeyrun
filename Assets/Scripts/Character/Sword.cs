using Combat;
using UnityEngine;

namespace Character
{
    public class Sword : MonoBehaviour
    {
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private Character character;
        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & enemyLayer) != 0)
            {
                if (character.IsAttacking)
                {
                    CombatManager.CharacterAttack(other.gameObject, character.GetSmallAttackDamage());
                    Debug.Log("Hit enemy: " + other.name);   
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
