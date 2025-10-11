using System.Collections;
using System.Linq;
using NPC;
using NPC.MovementBehaviours;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemy
{
    public class SlimeAI : AIAgent
    {
        private static readonly int IsSleeping = Animator.StringToHash("isSleeping");

        private static readonly int IsSearching = Animator.StringToHash("isSearching");

        private static readonly int IsWalking = Animator.StringToHash("isWalking");
        // Sequence:
        // 1. Idle - Sleep
        // 2. Idle - Wake up
        // 3. Search
        // 4. Wander for 3-5 sec
        // 5. Repeat steps 1 to 4
        
        // If at step 1 or 4, player enters slime detection radius:
        // 6. Sense something
        // 7. Run towards player
        
        // If at step 2 or 3, player enters slime detection radius:
        // 8. Taunt
        // 9. Walk towards player
        
        [Header("Slime Attributes")]
        [SerializeField] private float detectionRadius;

        [Header("Slime Behaviour Attributes")] 
        [SerializeField] private float sleepDuration;
        [SerializeField] private float awakeningDuration;
        [SerializeField] private float searchingDuration;
        [SerializeField] [Min(0)] private float minimumWanderDuration;
        [SerializeField] [Min(0)] private float maximumWanderDuration;
        
        private Animator _animator;
        
        private SlimeState currentState;
        
        private enum SlimeState
        {
            Sleeping,
            Awakening,
            Sensing,
            Wandering,
            Searching,
            Running,
            Taunting,
            Seeking
        }

        private IEnumerator BaseCycle()
        {
            while (true)
            {
                yield return Sleep();
                yield return WakeUp();
                yield return Search();
                yield return Wander();
            }
        }
        
        private IEnumerator Sleep()
        {
            currentState = SlimeState.Sleeping;
            _animator.SetBool(IsSleeping, true);
            _animator.SetBool(IsWalking, false);
            // Debug.Log("Slime sleeping...");
            yield return new WaitForSeconds(sleepDuration);
        }

        private IEnumerator WakeUp()
        {
            currentState = SlimeState.Awakening;
            _animator.SetBool(IsSleeping, false);
            // Debug.Log("Slime waking up...");
            yield return new WaitForSeconds(awakeningDuration);
        }

        private IEnumerator Search()
        {
            currentState = SlimeState.Searching;
            _animator.SetBool(IsSearching, true);
            // Debug.Log("Slime searching...");
            yield return new WaitForSeconds(searchingDuration);
        }

        private IEnumerator Wander()
        {
            currentState = SlimeState.Wandering;
            _animator.SetBool(IsWalking, true);
            _animator.SetBool(IsSearching, false);
            float duration = Random.Range(minimumWanderDuration, maximumWanderDuration);
            // Debug.Log($"Slime wandering for {duration} seconds...");
            float elapsed = 0f;

            while (elapsed < duration)
            {
                Move();
                
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            if (_animator == null)
            {
                Debug.LogError("No animator found");
            }
        }

        private void Start()
        {
            StartCoroutine(BaseCycle());
        }

        protected override void GetSteeringSum(out Vector3 steeringForceSum, out Quaternion rotation)
        {
            steeringForceSum = Vector3.zero;
            rotation = Quaternion.identity;
            
            AIMovement[] movements = GetComponents<AIMovement>();

            if (currentState == SlimeState.Wandering)
            {
                movements = movements.Where(m => m is Wander || m is FaceDirection).ToArray();
            }
            
            foreach (AIMovement movement in movements)
            {
                steeringForceSum += movement.GetSteering(this).Linear;
                rotation *= movement.GetSteering(this).Angular;
            }
        }

        protected override void Update()
        {
            
        }
    }
}
