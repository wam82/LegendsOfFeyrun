using System;
using Environment.Interfaces;
using UnityEngine;

namespace Environment.Objects
{
    public class Gem : MonoBehaviour, ICollectible
    {
        [Header("Objects to Rotate")]
        [SerializeField] private Transform topPiece;
        [SerializeField] private Transform bottomPiece;
        
        [Header("Rotation Speeds (degrees per second)")]
        [SerializeField] private float topSpeed;        
        [SerializeField] private float bottomSpeed;

        public bool Collected { get; set; }

        public void Collect()
        {
            Collected = true;
            Debug.Log("Gem Collected");
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (!Collected)
                {
                    Collect();
                }
            }
        }
        
        private void Update()
        {
            // Rotate around Y axis
            if (topPiece)
            {
                // Clockwise rotation = negative angle around Y
                topPiece.Rotate(0f, -topSpeed * Time.deltaTime, 0f, Space.Self);
            }

            if (bottomPiece)
            {
                // Counter-clockwise rotation = positive angle around Y
                bottomPiece.Rotate(0f, bottomSpeed * Time.deltaTime, 0f, Space.Self);
            }
        }
    }
}