using System.Linq;
using Character;
using NPC;
using NPC.MovementBehaviours;
using UnityEngine;

namespace Environment.Objects
{
    public class Key : AIAgent
    {
        [SerializeField] private float distanceToMaintain;
        [SerializeField] private float bufferDistance;
        
        private float _distance;
        
        private PlayableCharacter _playableCharacter;

        protected override void GetSteeringSum(out Vector3 steeringForceSum, out Quaternion rotation)
        {
            steeringForceSum = Vector3.zero;
            rotation = Quaternion.identity;

            AIMovement[] movements = GetComponents<AIMovement>();

            if (moveSpeed == 0)
            {
                movements = movements.Where(m => m is LookAt).ToArray();
            }
            else
            {
                movements = movements.Where(m => m is Seek || m is FaceDirection).ToArray();
            }
            
            foreach (AIMovement movement in movements)
            {
                steeringForceSum += movement.GetSteering(this).Linear;
                rotation *= movement.GetSteering(this).Angular;
            }
        }

        protected override void Start()
        {
            base.Start();
            _playableCharacter = trackedTarget.gameObject.GetComponent<PlayableCharacter>();   
        }

        protected override void Update()
        {
            base.Update();
            _distance = Vector3.Distance(transform.position, TargetPosition);
            moveSpeed = _playableCharacter.Speed + _distance/(distanceToMaintain + bufferDistance);

            if (_distance < distanceToMaintain - bufferDistance)
            {
                moveSpeed = 0;
            }
        }
    }
}