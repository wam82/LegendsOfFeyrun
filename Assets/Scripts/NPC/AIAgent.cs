using System.Collections.Generic;
using NPC.MovementBehaviours;
using UnityEngine;

namespace NPC
{
    public abstract class AIAgent : MonoBehaviour
    {
        public bool debug;
        public float maxSpeed;
        public Transform trackedTarget;
        private Vector3 _targetPosition;
        public List<GameObject> obstacles =  new();
        public Vector3 TargetPosition
        {
            get => trackedTarget != null ? trackedTarget.position : _targetPosition;
        }
        public Vector3 Velocity { get; set; }
        
        
        protected virtual void Move()
        {
            GetSteeringSum(out Vector3 steeringForceSum, out Quaternion rotation);
            Velocity += steeringForceSum * Time.deltaTime;
            Velocity = Vector3.ClampMagnitude(Velocity, maxSpeed);
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
            obstacles.AddRange(GameObject.FindGameObjectsWithTag("Obstacle"));
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
