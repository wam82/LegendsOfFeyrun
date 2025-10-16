using System.Collections;
using System.Linq;
using NPC;
using NPC.MovementBehaviours;
using Unity.VisualScripting;
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
        private static readonly int IsAttacking2 = Animator.StringToHash("isAttacking2");
        private static readonly int IsSneaking = Animator.StringToHash("isSneaking");
        private static readonly int IsTaunting = Animator.StringToHash("isTaunting");
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
        [SerializeField] private float walkSpeed;
        [SerializeField] private float sneakSpeed;
        [SerializeField] private float runSpeed;
        [SerializeField] private float detectionRadius;
        [SerializeField] private float attack1Radius;
        [SerializeField] private float attack2Radius;
        [SerializeField] private float attackCooldown;
        

        [Header("Slime Behaviour Attributes")] 
        [SerializeField] private float sleepDuration;
        [SerializeField] private float awakeningDuration;
        [SerializeField] private float searchingDuration;
        [SerializeField] private float sensingDuration;
        [SerializeField] private float tauntDuration;
        [SerializeField] [Min(0)] private float minimumWanderDuration;
        [SerializeField] [Min(0)] private float maximumWanderDuration;
        
        private Animator _animator;
        private Coroutine _activeBehaviour;
        
        private bool _inCombat;
        private bool _canAttack1 = true;
        private bool _canAttack2 = true;

        private float _attack1Time;
        private float _attack2Time;

        private enum SlimeState
        {
            Sleeping,
            Awakening,
            Sensing,
            Wandering,
            Searching,
            Running,
            Taunting,
            Sneaking,
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
            yield return new WaitForSeconds(sensingDuration);
        }

        private IEnumerator Taunt()
        {
            currentState = SlimeState.Taunting;
            ResetAllAnimatorBooleans();
            _animator.SetBool(IsTaunting, true);
            yield return new WaitForSeconds(tauntDuration);
        }

        private IEnumerator ChaseBehaviour1()
        {
            yield return Sense();
            ResetAllAnimatorBooleans();
            // _animator.SetBool(IsSensing, false);
            
            // Combat sequence:
            // 1. Sense
            // 2. Start combat loop:
            // Case 1: Player (Tracked Target) is no longer within detection radius --> Exit combat loop, player has escaped
            // Case 2: Player is outside attack range --> Enter running state, try to close the gap to the player (setting move speed to run speed). 
            // Case 3: Player is within attack range --> Enter attack state, stop movement, play attack animation.
            _inCombat = true;
            while (_inCombat)
            {
                if (Time.time - _attack1Time > attackCooldown)
                {
                    _canAttack1 =  true;
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

                if (distance > attack1Radius)
                {
                    ResetAllAnimatorBooleans();
                    currentState = SlimeState.Running;
                    moveSpeed = runSpeed;
                    Move();
                    _animator.SetBool(IsRunning, true);
                }
                else
                {
                    moveSpeed = 0f;
                    Move();
                    ResetAllAnimatorBooleans();
                    currentState = SlimeState.Attacking;
                    if (_canAttack1)
                    {
                        _animator.SetBool(IsAttacking1, true);
                        _canAttack1 = false;
                        _attack1Time = Time.time;
                    }
                }

                yield return null;
            }
        }

        private IEnumerator ChaseBehaviour2()
        {
            yield return Taunt();
            ResetAllAnimatorBooleans();
            _inCombat =  true;
            
            while (_inCombat)
            {
                if (Time.time - _attack2Time > attackCooldown)
                {
                    _canAttack2 =  true;
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
                if (distance > attack2Radius)
                {
                    ResetAllAnimatorBooleans();
                    currentState = SlimeState.Sneaking;
                    moveSpeed = sneakSpeed;
                    Move();
                    _animator.SetBool(IsSneaking, true);
                }
                else
                {
                    moveSpeed = 0f;
                    Move();
                    ResetAllAnimatorBooleans();
                    currentState = SlimeState.Attacking;
                    if (_canAttack2)
                    {
                        _animator.SetBool(IsAttacking2, true);
                        _canAttack2 = false;
                        _attack2Time = Time.time;
                    }
                }
                
                yield return null;
            }
        }
        
        public void OnAttack1AnimationConnection()
        {
            Debug.Log("Attack1 connected");
        }

        public void OnAttack2AnimationConnection()
        {
            Debug.Log("Attack2 connected");
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
            _animator.SetBool(IsTaunting, false);
            _animator.SetBool(IsRunning, false);
            _animator.SetBool(IsSneaking, false);
            _animator.SetBool(IsAttacking1, false);
            _animator.SetBool(IsAttacking2, false);
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

            if (currentState == SlimeState.Sneaking)
            {
                movements = movements.Where(m => m is Seek || m is FaceDirection || m is Avoid).ToArray();
            }

            if (currentState == SlimeState.Attacking)
            {
                movements = movements.Where(m => m is LookAt).ToArray();
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
            DebugUtils.DrawCircle(transform.position, Vector3.up, Color.red, attack1Radius);
            DebugUtils.DrawCircle(transform.position, Vector3.up, Color.cyan, attack2Radius);
            
            if (!_inCombat && PlayerDetected())
            {
                if (currentState == SlimeState.Sleeping || currentState == SlimeState.Wandering)
                {
                    SwitchCoroutine(ChaseBehaviour1());
                }

                if (currentState == SlimeState.Awakening || currentState == SlimeState.Searching)
                {
                    SwitchCoroutine(ChaseBehaviour2());
                }
            }
        }
    }
}
