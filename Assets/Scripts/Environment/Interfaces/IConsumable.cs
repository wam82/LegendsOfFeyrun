using UnityEngine;

namespace Environment.Interfaces
{
    public interface IConsumable
    {
        bool Consumed { get; set; }
        void Consume(GameObject gameObject);
        void OnTriggerEnter(Collider other);
    }
}