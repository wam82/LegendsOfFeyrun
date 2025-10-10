using UnityEngine;

namespace NPC.MovementBehaviours
{
    public class Wander : AIMovement
    {
        public float wanderAngle;
        public float wanderInterval;
        
        private float wanderTimer;

        private Vector3 lastDirection;
        private Vector3 lastMovement;

        public override SteeringOutput GetSteering(AIAgent agent)
        {
            SteeringOutput output = base.GetSteering(agent);
            wanderTimer += Time.deltaTime;

            if (lastDirection == Vector3.zero)
            {
                lastDirection = agent.transform.forward.normalized * agent.maxSpeed;
            }
            
            if (lastMovement == Vector3.zero)
            {
                lastMovement = agent.transform.forward;
            }

            Vector3 desiredVelocity = lastMovement;
            if (wanderTimer > wanderInterval)
            {
                float angle = (Random.value - Random.value) * wanderAngle;
                Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * lastDirection.normalized;
                Vector3 circleCenter = agent.transform.position + lastMovement;
                Vector3 destination = circleCenter + direction.normalized;

                desiredVelocity = destination - agent.transform.position;
                desiredVelocity = desiredVelocity.normalized * agent.maxSpeed;

                lastMovement = desiredVelocity;
                lastDirection = direction;
                wanderTimer = 0;
            }
            
            output.Linear = desiredVelocity - agent.Velocity;

            return output;
        }
    }
}