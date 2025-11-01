using Combat;
using Enemy.Agents;
using Enemy.Entities;
using UnityEngine;

namespace Enemy.Colliders
{
    public class SlimeCollider : MonoBehaviour
    {
        [SerializeField] private SlimeAI slimeAI;
        [SerializeField] private SlimeEntity slimeEntity;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                if (slimeAI.attack1Requested)
                {
                    CombatManager.SlimeAttack(other.gameObject, slimeEntity.Attack1Damage);
                    slimeAI.attack1Requested = false;
                }
                else if (slimeAI.attack2Requested)
                {
                    CombatManager.SlimeAttack(other.gameObject, slimeEntity.Attack2Damage);
                    slimeAI.attack2Requested = false;
                }
            }
        }
    }
}