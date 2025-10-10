using UnityEngine;

namespace Enemies.MovementBehaviours
{
    public class Seek : AIMovement
    {
        public override SteeringOutput GetSteering(SlimeAI agent)
        {
            SteeringOutput output = base.GetSteering(agent);
            Vector3 desiredVelocity = agent.TargetPosition - transform.position;
            desiredVelocity = desiredVelocity.normalized * agent.maxSpeed;
            Vector3 steering = desiredVelocity - agent.Velocity;
            output.Linear = steering;

            if (debug) Debug.DrawRay(transform.position + agent.Velocity, output.Linear, Color.green);

            return output;
        }
    }
}