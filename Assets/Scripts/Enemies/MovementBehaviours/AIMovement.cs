using UnityEngine;

namespace Enemies.MovementBehaviours
{
    public abstract class AIMovement : MonoBehaviour
    {
        public bool debug;

        public virtual SteeringOutput GetKinematic(SlimeAI agent)
        {
            return new SteeringOutput { Angular = agent.transform.rotation };
        }

        public virtual SteeringOutput GetSteering(SlimeAI agent)
        {
            return new SteeringOutput { Angular = Quaternion.identity };
        }
    }
}
