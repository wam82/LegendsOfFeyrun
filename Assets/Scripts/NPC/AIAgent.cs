using System.Collections.Generic;
using Combat;
using NPC.MovementBehaviours;
using UnityEngine;

namespace NPC
{
    public abstract class AIAgent : MonoBehaviour
    {
        public bool debug;
        public float moveSpeed;
        public Transform trackedTarget;
        private Vector3 _targetPosition;
        public List<GameObject> obstacles =  new();
        public Vector3 TargetPosition => trackedTarget ? trackedTarget.position : _targetPosition;
        public Vector3 Velocity { get; private set; }
        
        
        protected virtual void Move()
        {
            GetSteeringSum(out Vector3 steeringForceSum, out Quaternion rotation);
            Velocity += steeringForceSum * Time.deltaTime;
            Velocity = Vector3.ClampMagnitude(Velocity, moveSpeed);
            transform.position += Velocity * Time.deltaTime;
            transform.rotation *= rotation;
        }

        protected virtual void GetSteeringSum(out Vector3 steeringForceSum, out Quaternion rotation)
        {
            steeringForceSum = Vector3.zero;
            rotation = Quaternion.identity;
            
            AIMovement[] movements = GetComponents<AIMovement>();
            
            foreach (AIMovement movement in movements)
            {
                steeringForceSum += movement.GetSteering(this).Linear;
                rotation *= movement.GetSteering(this).Angular;
            }
        }

        protected virtual void Start()
        {
            if (obstacles.Count < 1)
            {
                obstacles.AddRange(GameObject.FindGameObjectsWithTag("Obstacle"));
            }
            
            if (!trackedTarget)
            {
                if (CombatManager.Instance.player != null)
                {
                    trackedTarget = CombatManager.Instance.player;
                }
                else
                {
                    Debug.LogError("No tracked target for " + gameObject.name);
                }
            }
        }

        protected virtual void Update()
        {
            if (debug)
            {
                Debug.DrawRay(transform.position, Velocity, Color.green);
            }

            Move();
        }
    }
}
