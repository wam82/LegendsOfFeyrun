using UnityEngine;

namespace Environment.Interfaces
{
    public interface ICollectible
    {
        bool Collected { get; set; }
        void Collect(GameObject player);
        void OnTriggerEnter(Collider other);
    }
}