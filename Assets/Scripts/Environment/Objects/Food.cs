using Character;
using Environment.Interfaces;
using UnityEngine;

namespace Environment.Objects
{
    public class Food : MonoBehaviour, IConsumable
    {
        [SerializeField] private float restoreAmount;

        public bool Consumed { get; set; }

        public void Consume(GameObject target)
        {
            target.GetComponent<PlayableCharacter>().RestoreHealth(restoreAmount);
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (!Consumed)
                {
                    Consume(other.gameObject);
                    Consumed = true;
                }
            }
        }
    }
}