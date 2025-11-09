using Character;
using Environment.Interfaces;
using UnityEngine;

namespace Environment.Objects
{
    public class Food : MonoBehaviour, ICollectible
    {
        [SerializeField] private float restoreAmount;

        public bool Collected { get; set; }

        public void Collect(GameObject target)
        {
            Collected = true;
            target.GetComponent<PlayableCharacter>().RestoreHealth(restoreAmount);
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (!Collected)
                {
                    Collect(other.gameObject);
                }
            }
        }
    }
}