using UnityEngine;
using UnityEngine.AI;

namespace Game.Core.Combat
{
    [RequireComponent(typeof(Animator))]
    public class EnemyAnimationController : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int InBattleHash = Animator.StringToHash("InBattle");
        private static readonly int ShootHash = Animator.StringToHash("Shoot");
        private static readonly int DeathHash = Animator.StringToHash("Death");
        private static readonly int TakeDamageHash = Animator.StringToHash("TakeDamage");
        private static readonly int VelocityXHash = Animator.StringToHash("x");
        private static readonly int VelocityYHash = Animator.StringToHash("y");
        
        private const int BASE_LAYER_INDEX = 0;
        private const int BATTLE_LAYER_INDEX = 1;
        private const float MOVEMENT_THRESHOLD = 0.1f;
        private const float MIN_AGENT_SPEED = 0.01f;
        
        [Header("Animation Settings")]
        [SerializeField] private float _speedSmoothTime = 0.1f;
        
        private Animator _animator;
        private NavMeshAgent _agent;
        private EnemyController _controller;
        
        private float _currentSpeed;
        private float _speedVelocity;
        private bool _isInBattle;
        
        public void Initialize(EnemyController controller)
        {
            _controller = controller;
            
            CacheComponents();
            ConfigureAgent();
            SetBattleState(true);
        }
        
        private void CacheComponents()
        {
            _animator = GetComponent<Animator>();
            
            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }
            
            _agent = GetComponent<NavMeshAgent>();
        }
        
        private void ConfigureAgent()
        {
            if (_agent != null)
            {
                _agent.updateRotation = false;
            }
        }
        
        private void Update()
        {
            if (_controller == null || !_controller.IsAlive) return;
            
            UpdateMovementSpeed();
            UpdateBattleState();
            
            if (_isInBattle)
            {
                UpdateBlendTreeParameters();
            }
        }
        
        private void UpdateMovementSpeed()
        {
            if (_agent == null || _animator == null) return;
            
            if (!_agent.isOnNavMesh)
            {
                SetIdleAnimation();
                return;
            }
            
            float targetSpeed = _agent.velocity.magnitude;
            _currentSpeed = Mathf.SmoothDamp(_currentSpeed, targetSpeed, ref _speedVelocity, _speedSmoothTime);
            
            bool isMoving = _currentSpeed > MOVEMENT_THRESHOLD;
            
            _animator.SetFloat(SpeedHash, _currentSpeed);
            _animator.SetBool(IsMovingHash, isMoving);
        }
        
        private void SetIdleAnimation()
        {
            _animator.SetFloat(SpeedHash, 0f);
            _animator.SetBool(IsMovingHash, false);
        }
        
        private void UpdateBlendTreeParameters()
        {
            if (_agent == null || _animator == null || !_agent.isOnNavMesh) 
                return;
            
            Vector3 localVelocity = transform.InverseTransformDirection(_agent.velocity);
            
            float normalizedX = localVelocity.x;
            float normalizedZ = localVelocity.z;
            
            if (_agent.speed > MIN_AGENT_SPEED)
            {
                normalizedX /= _agent.speed;
                normalizedZ /= _agent.speed;
            }
            
            _animator.SetFloat(VelocityXHash, normalizedX);
            _animator.SetFloat(VelocityYHash, normalizedZ);
        }
        
        private void UpdateBattleState()
        {
            if (_controller == null) return;
            
            bool shouldBeInBattle = _controller.IsInCombat;
            
            if (shouldBeInBattle != _isInBattle)
            {
                SetBattleState(shouldBeInBattle);
            }
        }
        
        private void SetBattleState(bool inBattle)
        {
            _isInBattle = inBattle;
            
            if (_animator == null) return;
            
            _animator.SetBool(InBattleHash, inBattle);
            
            float battleWeight = inBattle ? 1f : 0f;
            float baseWeight = inBattle ? 0f : 1f;
            
            _animator.SetLayerWeight(BATTLE_LAYER_INDEX, battleWeight);
            _animator.SetLayerWeight(BASE_LAYER_INDEX, baseWeight);
        }
        
        public void PlayShootAnimation()
        {
            if (_animator != null)
            {
                _animator.SetTrigger(ShootHash);
            }
        }
        
        public void PlayTakeDamageAnimation()
        {
            if (_animator != null)
            {
                _animator.SetTrigger(TakeDamageHash);
            }
        }
        
        public void PlayDeathAnimation()
        {
            if (_animator != null)
            {
                _animator.SetTrigger(DeathHash);
            }
        }
    }
}