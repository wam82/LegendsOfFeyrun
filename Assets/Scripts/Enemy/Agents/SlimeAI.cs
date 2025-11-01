using System.Collections;
using System.Linq;
using Combat;
using Enemy.Entities;
using NPC;
using NPC.MovementBehaviours;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemy.Agents
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
        private static readonly int IsDead = Animator.StringToHash("isDead");
        private static readonly int IsHurt = Animator.StringToHash("isHurt");
        
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
        private SlimeEntity _entity;
        
        private bool _inCombat;
        private bool _canAttack1 = true;
        private bool _canAttack2 = true;
        private bool _hasTaunted;
        public bool attack1Requested;
        public bool attack2Requested;

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
            Attacking,
            Hurt,
            Dead
        }

        private IEnumerator BaseBehaviour()
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
            ResetAllAnimatorParameters();
            _animator.SetBool(IsSleeping, true);
            yield return new WaitForSeconds(sleepDuration);
        }

        private IEnumerator WakeUp()
        {
            currentState = SlimeState.Awakening;
            ResetAllAnimatorParameters();
            yield return new WaitForSeconds(awakeningDuration);
        }

        private IEnumerator Search()
        {
            currentState = SlimeState.Searching;
            ResetAllAnimatorParameters();
            _animator.SetBool(IsSearching, true);
            yield return new WaitForSeconds(searchingDuration);
        }

        private IEnumerator Wander()
        {
            currentState = SlimeState.Wandering;
            ResetAllAnimatorParameters();
            _animator.SetBool(IsWalking, true);
            float duration = Random.Range(minimumWanderDuration, maximumWanderDuration);
            float elapsed = 0f;
            moveSpeed = _entity.WalkSpeed;

            while (elapsed < duration)
            {
                Move();
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator Sense()
        {
            currentState = SlimeState.Sensing;
            ResetAllAnimatorParameters();
            _animator.SetBool(IsSensing, true);
            yield return new WaitForSeconds(sensingDuration);
        }

        private IEnumerator Taunt()
        {
            currentState = SlimeState.Taunting;
            ResetAllAnimatorParameters();
            _animator.SetBool(IsTaunting, true);
            yield return new WaitForSeconds(tauntDuration);
        }

        public void Die()
        {
            currentState = SlimeState.Dead;
            if (_activeBehaviour != null)
            {
                StopCoroutine(_activeBehaviour);
            }
            ResetAllAnimatorParameters();
            _animator.SetBool(IsDead, true);
        }

        public void GetHurt()
        {
            currentState = SlimeState.Hurt;
            if (_activeBehaviour != null)
            {
                StopCoroutine(_activeBehaviour);
            }
            ResetAllAnimatorParameters();
            _inCombat = false;
            _animator.SetTrigger(IsHurt);
        }

        private IEnumerator ChaseBehaviour1()
        {
            // Combat sequence:
            // 1. Sense
            // 2. Start combat loop:
            // Case 1: Player (Tracked Target) is no longer within detection radius --> Exit combat loop, player has escaped
            // Case 2: Player is outside attack range --> Enter running state, try to close the gap to the player (setting move speed to run speed). 
            // Case 3: Player is within attack range --> Enter attack state, stop movement, play attack animation.
            
            yield return Sense();
            ResetAllAnimatorParameters();
            _inCombat = true;
            while (_inCombat)
            {
                if (Time.time - _attack1Time > _entity.AttackCooldown)
                {
                    _canAttack1 =  true;
                }
                
                if (!PlayerDetected())
                {
                    _inCombat = false;
                    ResetAllAnimatorParameters();
                    _animator.SetBool(IsSleeping, true);
                    SwitchCoroutine(BaseBehaviour());
                    yield break;
                }
                
                float distance = Vector3.Distance(transform.position, TargetPosition);

                if (distance > _entity.Attack1Radius)
                {
                    ResetAllAnimatorParameters();
                    currentState = SlimeState.Running;
                    moveSpeed = _entity.RunSpeed;
                    Move();
                    _animator.SetBool(IsRunning, true);
                }
                else
                {
                    moveSpeed = 0f;
                    Move();
                    ResetAllAnimatorParameters();
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
            // Combat sequence:
            // 1. Taunt
            // 2. Start combat loop:
            // Case 1: Player (Tracked Target) is no longer within detection radius --> Exit combat loop, player has escaped
            // Case 2: Player is outside attack range --> Enter sneak state, try to close the gap to the player (setting move speed to sneak speed). 
            // Case 3: Player is within attack range --> Enter attack state, stop movement, play attack animation.
            
            if (!_hasTaunted)
            {
                _hasTaunted = true;
                yield return Taunt();
            }
            
            ResetAllAnimatorParameters();
            _inCombat =  true;
            
            while (_inCombat)
            {
                if (Time.time - _attack2Time > _entity.AttackCooldown)
                {
                    _canAttack2 =  true;
                }
                
                if (!PlayerDetected())
                {
                    _hasTaunted = false;
                    _inCombat = false;
                    ResetAllAnimatorParameters();
                    _animator.SetBool(IsSleeping, true);
                    SwitchCoroutine(BaseBehaviour());
                    yield break;
                }
                
                float distance = Vector3.Distance(transform.position, trackedTarget.position);
                if (distance > _entity.Attack2Radius)
                {
                    ResetAllAnimatorParameters();
                    currentState = SlimeState.Sneaking;
                    moveSpeed = _entity.SneakSpeed;
                    Move();
                    _animator.SetBool(IsSneaking, true);
                }
                else
                {
                    moveSpeed = 0f;
                    Move();
                    ResetAllAnimatorParameters();
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
            attack1Requested  = true;
        }

        // public void OnAttack1AnimationCompleted()
        // {
        //     attack1Requested = false;
        // }

        public void OnAttack2AnimationConnection()
        {
            attack2Requested = true;
        }
        
        // public void OnAttack2AnimationCompleted()
        // {
        //     attack2Requested = false;
        // }

        public void OnHurtAnimationComplete()
        {
            ResetAllAnimatorParameters();
            SwitchCoroutine(ChaseBehaviour2());
        }
        
        private bool PlayerDetected()
        {
            if (!trackedTarget)
            {
                return false;
            }
            float dist = Vector3.Distance(transform.position, TargetPosition);
            return dist < _entity.DetectionRadius;
        }
        
        private void ResetAllAnimatorParameters()
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
            _animator.ResetTrigger(IsHurt);
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

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            if (_animator == null)
            {
                Debug.LogError("No animator found");
            }

            _entity = GetComponent<SlimeEntity>();
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
            DebugUtils.DrawCircle(transform.position, Vector3.up, Color.yellow, _entity.DetectionRadius);
            DebugUtils.DrawCircle(transform.position, Vector3.up, Color.red, _entity.Attack1Radius);
            DebugUtils.DrawCircle(transform.position, Vector3.up, Color.cyan, _entity.Attack2Radius);
            
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
