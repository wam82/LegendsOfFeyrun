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
        private static readonly int IsSensing = Animator.StringToHash("isSensing");
        private static readonly int IsRunning = Animator.StringToHash("isRunning");

        private static readonly int IsAttacking1 = Animator.StringToHash("isAttacking1");
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
        
        [SerializeField] private SlimeState currentState;
        
        [Header("Slime Attributes")]
        [SerializeField] private float detectionRadius;
        [SerializeField] private float attackRadius;
        [SerializeField] private float attackCooldown;
        [SerializeField] private float walkSpeed;
        [SerializeField] private float runSpeed;

        [Header("Slime Behaviour Attributes")] 
        [SerializeField] private float sleepDuration;
        [SerializeField] private float awakeningDuration;
        [SerializeField] private float searchingDuration;
        [SerializeField] [Min(0)] private float minimumWanderDuration;
        [SerializeField] [Min(0)] private float maximumWanderDuration;
        
        private Animator _animator;
        private Coroutine _activeBehaviour;
        
        private bool _inCombat;
        private bool _canAttack = true;

        private float _attack1Time;
        
        private enum SlimeState
        {
            Sleeping,
            Awakening,
            Sensing,
            Wandering,
            Searching,
            Running,
            Taunting,
            Seeking,
            Attacking
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
            ResetAllAnimatorBooleans();
            _animator.SetBool(IsSleeping, true);
            // Debug.Log("Slime sleeping...");
            yield return new WaitForSeconds(sleepDuration);
        }

        private IEnumerator WakeUp()
        {
            currentState = SlimeState.Awakening;
            ResetAllAnimatorBooleans();
            // _animator.SetBool(IsSleeping, false);
            // Debug.Log("Slime waking up...");
            yield return new WaitForSeconds(awakeningDuration);
        }

        private IEnumerator Search()
        {
            currentState = SlimeState.Searching;
            ResetAllAnimatorBooleans();
            _animator.SetBool(IsSearching, true);
            // Debug.Log("Slime searching...");
            yield return new WaitForSeconds(searchingDuration);
        }

        private IEnumerator Wander()
        {
            currentState = SlimeState.Wandering;
            ResetAllAnimatorBooleans();
            _animator.SetBool(IsWalking, true);
            float duration = Random.Range(minimumWanderDuration, maximumWanderDuration);
            // Debug.Log($"Slime wandering for {duration} seconds...");
            float elapsed = 0f;
            moveSpeed = walkSpeed;

            while (elapsed < duration)
            {
                Move();

                // if (PlayerDetected())
                // {
                //     _animator.SetBool(IsWalking, false);
                //     SwitchCoroutine(ChaseBehaviour1());
                //     yield break;
                // }
                
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator Sense()
        {
            currentState = SlimeState.Sensing;
            ResetAllAnimatorBooleans();
            _animator.SetBool(IsSensing, true);
            yield return new WaitForSeconds(1f);
        }

        private IEnumerator ChaseBehaviour1()
        {
            yield return Sense();
            ResetAllAnimatorBooleans();
            // _animator.SetBool(IsSensing, false);
            
            // Combat sequence:
            // 1. Sense (Done above)
            // 2. Start combat loop:
            // Case 1: Player (Tracked Target) is no longer within detection radius --> Exit combat loop, player has escaped
            // Case 2: Player is outside attack range --> Enter running state, try to close the gap to the player (setting move speed to run speed). 
            // Case 3: Player is within attack range --> Enter attack state (maybe?), stop movement, play attack animation. Attack logic TBD
            _inCombat = true;
            while (_inCombat)
            {
                if (Time.time - _attack1Time > attackCooldown)
                {
                    _canAttack =  true;
                }
                
                if (!PlayerDetected())
                {
                    _inCombat = false;
                    ResetAllAnimatorBooleans();
                    _animator.SetBool(IsSleeping, true);
                    SwitchCoroutine(BaseCycle());
                    yield break;
                }
                
                float distance = Vector3.Distance(transform.position, trackedTarget.position);

                if (distance > attackRadius)
                {
                    ResetAllAnimatorBooleans();
                    currentState = SlimeState.Running;
                    moveSpeed = runSpeed;
                    Move();
                    _animator.SetBool(IsRunning, true);
                }
                else
                {
                    ResetAllAnimatorBooleans();
                    currentState = SlimeState.Attacking;
                    if (_canAttack)
                    {
                        _animator.SetBool(IsAttacking1, true);
                        _canAttack = false;
                        _attack1Time = Time.time;
                    }
                }

                yield return null;
            }
        }
        
        public void OnAttack1AnimationConnection()
        {
            Debug.Log("Attack1 connected");
        }

        
        private bool PlayerDetected()
        {
            if (trackedTarget == null)
            {
                return false;
            }
            float dist = Vector3.Distance(transform.position, trackedTarget.position);
            return dist < detectionRadius;
        }
        
        private void ResetAllAnimatorBooleans()
        {
            _animator.SetBool(IsSleeping, false);
            _animator.SetBool(IsSearching, false);
            _animator.SetBool(IsWalking, false);
            _animator.SetBool(IsSensing, false);
            _animator.SetBool(IsRunning, false);
            _animator.SetBool(IsAttacking1, false);
        }

        private void SwitchCoroutine(IEnumerator newBehaviour)
        {
            if (_activeBehaviour != null)
            {
                StopCoroutine(_activeBehaviour);
            }

            ResetAllAnimatorBooleans();
            _activeBehaviour = StartCoroutine(newBehaviour);
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            if (_animator == null)
            {
                Debug.LogError("No animator found");
            }
        }

        protected override void Start()
        {
            base.Start();
            _activeBehaviour = StartCoroutine(BaseCycle());
        }

        protected override void GetSteeringSum(out Vector3 steeringForceSum, out Quaternion rotation)
        {
            steeringForceSum = Vector3.zero;
            rotation = Quaternion.identity;
            
            AIMovement[] movements = GetComponents<AIMovement>();

            if (currentState == SlimeState.Wandering)
            {
                movements = movements.Where(m => m is Wander || m is FaceDirection || m is Avoid).ToArray();
            }

            if (currentState == SlimeState.Running)
            {
                movements = movements.Where(m => m is Seek || m is FaceDirection || m is Avoid).ToArray();
            }

            if (currentState == SlimeState.Attacking)
            {
                movements = movements.Where(m => m is FaceDirection).ToArray();
            }
            
            foreach (AIMovement movement in movements)
            {
                steeringForceSum += movement.GetSteering(this).Linear;
                rotation *= movement.GetSteering(this).Angular;
            }
        }

        protected override void Update()
        {
            DebugUtils.DrawCircle(transform.position, Vector3.up, Color.yellow, detectionRadius);
            DebugUtils.DrawCircle(transform.position, Vector3.up, Color.red, attackRadius);
            
            if (!_inCombat && PlayerDetected())
            {
                if (currentState == SlimeState.Sleeping || currentState == SlimeState.Wandering)
                {
                    SwitchCoroutine(ChaseBehaviour1());
                }
            }
        }
    }
}
