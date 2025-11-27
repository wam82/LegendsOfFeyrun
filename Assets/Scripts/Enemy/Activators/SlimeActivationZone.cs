using System.Collections.Generic;
using Enemy.Agents;
using UnityEngine;

namespace Enemy.Activators
{
    public class SlimeActivationZone : MonoBehaviour
    {
        [SerializeField] private List<SlimeAI> slimes = new List<SlimeAI>();

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            foreach (SlimeAI slime in slimes)
            {
                if (slime != null)
                {
                    slime.Activate();
                }
            }
        }
    }
}