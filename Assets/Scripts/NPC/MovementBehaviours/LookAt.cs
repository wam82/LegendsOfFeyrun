using UnityEngine;

namespace NPC.MovementBehaviours
{
    public class LookAt : AIMovement
    {
        public override SteeringOutput GetSteering(AIAgent agent)
        {
            SteeringOutput output = base.GetSteering(agent);

            if (!agent.trackedTarget)
            {
                output.Angular = Quaternion.identity;
                return output;
            }
            
            Vector3 toTarget = agent.trackedTarget.transform.position - agent.transform.position;
            toTarget.y = 0;
            
            if (toTarget.sqrMagnitude < 0.001f)
            {
                output.Angular = Quaternion.identity;
                return output;
            }
            
            Quaternion targetRotation = Quaternion.LookRotation(toTarget.normalized);
            
            Vector3 from = Vector3.ProjectOnPlane(agent.transform.forward, Vector3.up);
            Vector3 to = targetRotation * Vector3.forward;
            float angleY = Vector3.SignedAngle(from, to, Vector3.up);

            output.Angular = Quaternion.AngleAxis(angleY, Vector3.up);
            
            return output;
        }
    }
}