using UnityEngine;

namespace NPC.MovementBehaviours
{
    public class SumoAvoid : AIMovement
    {
        public float detectionDistance;
        public float minimumTurnAngle;
        public float maximumTurnAngle;
        public float avoidanceFactor;

        public LayerMask wallLayer;

        private bool _isTurning;
        private Vector3 _targetDirection;
        private Quaternion _targetRotation;

        public override SteeringOutput GetSteering(AIAgent agent)
        {
            if (debug)
            {
                Debug.DrawRay(transform.position, agent.transform.forward * detectionDistance, Color.magenta);
            }
            SteeringOutput output = base.GetSteering(agent);
            
            if (Physics.Raycast(agent.transform.position + Vector3.up * 0.5f, agent.transform.forward, detectionDistance, wallLayer))
            {
                float angle = Random.Range(minimumTurnAngle, maximumTurnAngle);
                
                _targetDirection = Quaternion.AngleAxis(angle, Vector3.up) * agent.transform.forward;
                
                _targetRotation = Quaternion.LookRotation(_targetDirection, Vector3.up);
                
                _isTurning = true;
            }
            
            if (_isTurning)
            {
                if (Quaternion.Angle(agent.transform.rotation, _targetRotation) < 1f)
                {
                    _isTurning = false;
                }

                output.Linear = _targetDirection.normalized * avoidanceFactor;
            }
            
            return output;
        }
    }
}