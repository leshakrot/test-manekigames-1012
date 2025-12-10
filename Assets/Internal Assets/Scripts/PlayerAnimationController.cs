using UnityEngine;
using UnityEngine.AI;

namespace Game.Core.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationController : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int InBattleHash = Animator.StringToHash("InBattle");
        private static readonly int DeathHash = Animator.StringToHash("Death");
        private static readonly int TakeDamageHash = Animator.StringToHash("TakeDamage");
        private static readonly int VelocityXHash = Animator.StringToHash("x");
        private static readonly int VelocityYHash = Animator.StringToHash("y");
        
        private const int BASE_LAYER_INDEX = 0;
        private const int BATTLE_LAYER_INDEX = 1;
        private const float MOVEMENT_THRESHOLD = 0.01f;
        
        [Header("Animation Settings")]
        [SerializeField] private float _speedSmoothTime = 0.1f;
        [SerializeField] private float _combatDetectionRange = 15f;
        [SerializeField] private float _rotationSpeed = 10f;
        
        private Animator _animator;
        private NavMeshAgent _agent;
        
        private float _currentSpeed;
        private float _speedVelocity;
        private bool _isInBattle;
        private Combat.EnemyController _closestEnemy;
        
        private float _combatRangeSqr;
        
        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _agent = GetComponent<NavMeshAgent>();
            
            _combatRangeSqr = _combatDetectionRange * _combatDetectionRange;
            
            SetBattleState(false);
        }
        
        private void Update()
        {
            UpdateAnimationParameters();
            UpdateBattleState();
            
            if (_isInBattle)
            {
                UpdateBlendTreeParameters();
                
                if (_closestEnemy != null && _closestEnemy.IsAlive)
                {
                    LookAtTarget(_closestEnemy.transform);
                }
            }
        }
        
        private void UpdateAnimationParameters()
        {
            float targetSpeed = CalculateTargetSpeed();
            _currentSpeed = Mathf.SmoothDamp(_currentSpeed, targetSpeed, ref _speedVelocity, _speedSmoothTime);
            
            bool isMoving = targetSpeed > MOVEMENT_THRESHOLD;
            
            _animator.SetFloat(SpeedHash, _currentSpeed);
            _animator.SetBool(IsMovingHash, isMoving);
        }
        
        private float CalculateTargetSpeed()
        {
            if (_agent != null)
            {
                return _agent.velocity.magnitude;
            }
            
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            return new Vector3(horizontal, 0f, vertical).magnitude;
        }
        
        private void UpdateBattleState()
        {
            bool hasEnemiesNearby = CheckForEnemiesNearby();
            
            if (hasEnemiesNearby != _isInBattle)
            {
                SetBattleState(hasEnemiesNearby);
            }
        }
        
        private bool CheckForEnemiesNearby()
        {
            if (Combat.EnemyRegistry.Instance == null)
                return false;
            
            _closestEnemy = Combat.EnemyRegistry.Instance.FindClosestEnemy(
                transform.position, 
                _combatDetectionRange
            );
            
            return _closestEnemy != null;
        }
        
        private void UpdateBlendTreeParameters()
        {
            if (_agent == null || !_isInBattle) return;
            
            Vector3 localVelocity = transform.InverseTransformDirection(_agent.velocity);
            
            float normalizedX = localVelocity.x;
            float normalizedZ = localVelocity.z;
            
            if (_agent.speed > MOVEMENT_THRESHOLD)
            {
                normalizedX /= _agent.speed;
                normalizedZ /= _agent.speed;
            }
            
            _animator.SetFloat(VelocityXHash, normalizedX);
            _animator.SetFloat(VelocityYHash, normalizedZ);
        }
        
        private void LookAtTarget(Transform target)
        {
            if (target == null) return;
            
            Vector3 direction = target.position - transform.position;
            direction.y = 0;
            
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, 
                    targetRotation, 
                    _rotationSpeed * Time.deltaTime
                );
            }
        }
        
        private void SetBattleState(bool inBattle)
        {
            _isInBattle = inBattle;
            
            if (_agent != null)
            {
                _agent.updateRotation = !inBattle;
            }
            
            _animator.SetBool(InBattleHash, inBattle);
            
            float battleWeight = inBattle ? 1f : 0f;
            float baseWeight = inBattle ? 0f : 1f;
            
            _animator.SetLayerWeight(BATTLE_LAYER_INDEX, battleWeight);
            _animator.SetLayerWeight(BASE_LAYER_INDEX, baseWeight);
        }
        
        public void PlayDeathAnimation()
        {
            _animator.SetTrigger(DeathHash);
        }
        
        public void PlayTakeDamageAnimation()
        {
            _animator.SetTrigger(TakeDamageHash);
        }
    }
}