using UnityEngine;

namespace Environment.Interfaces
{
    public interface ICollectible
    {
        bool Collected { get; set; }
        void Collect();
        void OnTriggerEnter(Collider other);
    }
}