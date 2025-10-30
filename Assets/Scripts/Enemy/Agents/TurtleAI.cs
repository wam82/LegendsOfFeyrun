using System.Collections;
using System.Linq;
using Combat;
using Enemy.Entities;
using NPC;
using NPC.MovementBehaviours;
using UnityEngine;

namespace Enemy.Agents
{
    public class TurtleAI : AIAgent
    {
        private static readonly int IsWalking = Animator.StringToHash("isWalking");
        private static readonly int IsMeleeAttacking = Animator.StringToHash("isMeleeAttacking");
        private static readonly int IsRangeAttacking = Animator.StringToHash("isRangeAttacking");
        private static readonly int IsRunning = Animator.StringToHash("isRunning");
        private static readonly int IsDead = Animator.StringToHash("isDead");
        private static readonly int IsHurt = Animator.StringToHash("isHurt");
        private static readonly int IsDefending = Animator.StringToHash("isDefending");

        // Sequence:
        // 1. Walk "forward" until you're "in range" of a wall (forward direction + X units in front for the check) 
        // 2. Rotate right by a random angle between 90° & 180° (needs to be a check at each frame that calls rotate method instead of a simple boolean condition)
        
        // If player is within melee range (forward direction + X unit in front for the check)
        // 3. Melee attack (attack 01)
        
        // If player is not within melee range
        // 4. Range attack (attack 02)
        
        // NOTE: Attacks are continuous (with a small cooldown) as long as player is detected and turtle should be able to swap between attacks.
        // NOTE: Attacking a player is always more important than avoiding a wall.
        
        [SerializeField] private TurtleState currentState = TurtleState.Idle;
        
        [Header("Turtle Behaviour Settings")]
        [SerializeField] private float detectionRadius;
        [SerializeField] private float detectionAngle;
        [SerializeField] private LayerMask obstacleLayerMask;

        private bool _inCombat;
        private bool _canAttack = true;

        private float _lastAttackTime;
        
        private Animator _animator;
        private Coroutine _activeBehaviour;
        private TurtleEntity _entity;
        
        
        private enum TurtleState
        {
            Walking,
            Running,
            Idle,
            Melee,
            Ranged,
            Dead,
            Hurt,
            Defending
        }

        private IEnumerator BaseBehaviour()
        {
            currentState = TurtleState.Walking;
            ResetAllAnimatorParameters();
            _animator.SetBool(IsWalking, true);
            moveSpeed =  _entity.WalkSpeed;

            while (true)
            {
                Move();
                
                yield return null;
            }
        }

        private IEnumerator CombatBehaviour()
        {
            ResetAllAnimatorParameters();
            _inCombat = true;

            while (_inCombat)
            {
                if (Time.time - _lastAttackTime > _entity.AttackCooldown)
                {
                    _canAttack = true;
                }

                if (!PlayerDetected())
                {
                    _inCombat = false;
                    ResetAllAnimatorParameters();
                    _animator.SetBool(IsWalking, true);
                    SwitchCoroutine(BaseBehaviour());
                    yield break;
                }

                float distance = Vector3.Distance(transform.position, TargetPosition);

                if (distance > _entity.RangedAttackRadius)
                {
                    // Run to try and catch up
                    ResetAllAnimatorParameters();
                    currentState = TurtleState.Running;
                    moveSpeed = _entity.RunSpeed;
                    Move();
                    _animator.SetBool(IsRunning, true);
                }
                else if (distance <= _entity.RangedAttackRadius && distance > _entity.MeleeAttackRadius)
                {
                    // Ranged attack
                    moveSpeed = 0f;
                    Move();
                    ResetAllAnimatorParameters();
                    currentState = TurtleState.Ranged;
                    if (_canAttack)
                    {
                        _animator.SetBool(IsRangeAttacking, true);
                        _canAttack = false;
                        _lastAttackTime = Time.time;
                    }
                }
                else if (distance <=  _entity.MeleeAttackRadius)
                {
                    // Melee attack
                    moveSpeed = 0f;
                    Move();
                    ResetAllAnimatorParameters();
                    currentState = TurtleState.Melee;
                    if (_canAttack)
                    {
                        _animator.SetBool(IsMeleeAttacking, true);
                        _canAttack = false;
                        _lastAttackTime = Time.time;
                    }
                }
                else
                {
                    Debug.Log("Something went wrong");
                }
                
                yield return null;
            }
        }
        
        public void Die()
        {
            currentState = TurtleState.Dead;
            if (_activeBehaviour != null)
            {
                StopCoroutine(_activeBehaviour);
            }
            ResetAllAnimatorParameters();
            _animator.SetBool(IsDead, true);
        }

        public void GetHurt()
        {
            currentState = TurtleState.Hurt;
            if (_activeBehaviour != null)
            {
                StopCoroutine(_activeBehaviour);
            }
            ResetAllAnimatorParameters();
            _inCombat = false;
            _animator.SetTrigger(IsHurt);
        }

        public void OnHurtAnimationComplete()
        {
            ResetAllAnimatorParameters();
            SwitchCoroutine(BaseBehaviour());
        }
        
        private bool PlayerDetected()
        {
            // To check if the player is detected:
            // 1. Check if distance to the player is smaller than position.forward + detection range
            // 2. Check if player position is within forward ± field of view angle
            // 3. Check if light of sight to player is unobstructed
            
            if (!trackedTarget)
            {
                // Debug.LogError("No tracked target found");
                return false;
            }
            
            float distance = Vector3.Distance(transform.position, TargetPosition);

            if (distance > detectionRadius)
            {
                return false;
            }
            
            Vector3 direction = TargetPosition - transform.position;
            float angle = Vector3.Angle(transform.forward, direction);

            if (Mathf.Abs(angle) > detectionAngle)
            {
                return false;
            }

            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, direction.normalized, out RaycastHit hit, distance, obstacleLayerMask))
            {
                if (hit.transform != trackedTarget)
                {
                    return false;
                }
            }

            return true;
        }
        
        private void ResetAllAnimatorParameters()
        {
            _animator.SetBool(IsWalking, false);
            _animator.SetBool(IsRunning, false);
            _animator.SetBool(IsMeleeAttacking, false);
            _animator.SetBool(IsRangeAttacking, false);
            _animator.ResetTrigger(IsHurt);
            _animator.ResetTrigger(IsDefending);
        }
        
        private void SwitchCoroutine(IEnumerator newBehaviour)
        {
            if (_activeBehaviour != null)
            {
                StopCoroutine(_activeBehaviour);
            }

            ResetAllAnimatorParameters();
            _activeBehaviour = StartCoroutine(newBehaviour);
        }
        
        protected override void GetSteeringSum(out Vector3 steeringForceSum, out Quaternion rotation)
        {
            steeringForceSum = Vector3.zero;
            rotation = Quaternion.identity;
            
            AIMovement[] movements = GetComponents<AIMovement>();

            if (currentState == TurtleState.Walking)
            {
                movements = movements.Where(m => m is Wander || m is SumoAvoid || m is FaceDirection).ToArray();
            }

            if (currentState is TurtleState.Melee or TurtleState.Ranged)
            {
                movements = movements.Where(m => m is LookAt).ToArray();
            }

            if (currentState == TurtleState.Running)
            {
                movements = movements.Where(m => m is Seek || m is Avoid || m is FaceDirection).ToArray();
            }
        
            foreach (AIMovement movement in movements)
            {
                steeringForceSum += movement.GetSteering(this).Linear;
                rotation *= movement.GetSteering(this).Angular;
            }
        }
        
        private void Awake()
        {
            _animator = GetComponent<Animator>();
            if (_animator == null)
            {
                Debug.LogError("No animator found");
            }
            
            _entity = GetComponent<TurtleEntity>();
            if (_entity == null)
            {
                Debug.LogError("No creature entity found");
            }
        }
        
        protected override void Start()
        {
            base.Start();
            
            if (!trackedTarget)
            {
                if (CombatManager.Instance.player != null)
                {
                    trackedTarget = CombatManager.Instance.player;
                }
                else
                {
                    Debug.LogError("No tracked target for " + gameObject.name);
                }
            }
            
            _activeBehaviour = StartCoroutine(BaseBehaviour());
        }
        protected override void Update()
        {
            if (!_inCombat && PlayerDetected())
            {
                SwitchCoroutine(CombatBehaviour());
            }
        }
    }
}
