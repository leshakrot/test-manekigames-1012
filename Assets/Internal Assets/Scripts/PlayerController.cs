using UnityEngine;
using UnityEngine.AI;
using System;
using Game.Core;
using Game.Core.Combat;
using Game.Core.UI;
using Game.Core.Obstacles;
using Game.Core.Audio;
using Game.Core.MobileInput;
using UnityInput = UnityEngine.Input;

namespace Game.Core.Player
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class PlayerController : MonoBehaviour
    {
        private const float MOVEMENT_THRESHOLD = 0.1f;
        private const float ROTATION_ANGLE_CONVERSION = 60f;
        
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _rotationSpeed = 10f;
        
        [Header("Animation")]
        [SerializeField] private Animator _animator;
        [SerializeField] private RuntimeAnimatorController _defaultAnimatorController;
        
        [Header("Combat")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _attackDamage = 20f;
        [SerializeField] private float _attackCooldown = 0.6f;
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private float _projectileSpeed = 8f;   
        [SerializeField] private float _projectileSpawnOffset = 0.5f;
        
        [Header("UI")]
        [SerializeField] private HealthBar _healthBar;
        
        [Header("Audio")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _hitSound;
        [SerializeField] private AudioClip _deathSound;
        
        private NavMeshAgent _agent;
        private AttackEffect _attackEffect;
        private DamageFlash _damageFlash;
        private PlayerAnimationController _animationController;
        private Weapon _weapon;
        
        private bool _controlEnabled = true;
        private float _currentHealth;
        private float _lastAttackTime;
        private RoadController _currentRoad;
        private float _currentSpeedModifier = 1f;
        
        private bool _canUpdate;
        
        public event Action OnReachedFinish;
        public event Action OnDeath;
        public event Action<float, float> OnHealthChanged;
        
        public float Health => _currentHealth;
        public float MaxHealth => _maxHealth;
        public bool IsAlive => _currentHealth > 0;
        
        public void Initialize()
        {
            CacheComponents();
            InitializeAgent();
            InitializeAudio();
            InitializeHealth();
            
            _controlEnabled = true;
        }
        
        private void CacheComponents()
        {
            _agent = GetComponent<NavMeshAgent>();
            _attackEffect = GetComponent<AttackEffect>();
            _damageFlash = GetComponent<DamageFlash>();
            _animationController = GetComponent<PlayerAnimationController>();
            _weapon = GetComponentInChildren<Weapon>();
            
            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }
            
            if (_animator != null && _defaultAnimatorController == null)
            {
                _defaultAnimatorController = _animator.runtimeAnimatorController;
            }
        }
        
        private void InitializeAgent()
        {
            if (_agent != null)
            {
                _agent.speed = _moveSpeed;
                _agent.angularSpeed = _rotationSpeed * ROTATION_ANGLE_CONVERSION;
                _agent.stoppingDistance = 0.1f;
            }
        }
        
        private void InitializeAudio()
        {
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
            }
            
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
                _audioSource.spatialBlend = 1f;
            }
        }
        
        private void InitializeHealth()
        {
            _currentHealth = _maxHealth;
            
            if (_healthBar != null)
            {
                _healthBar.Initialize(transform);
                _healthBar.UpdateHealth(_currentHealth, _maxHealth);
            }
        }
        
        private void Update()
        {
            UpdateCanUpdate();
            
            if (!_canUpdate) return;
            
            HandleMovement();
        }
        
        private void UpdateCanUpdate()
        {
            _canUpdate = _controlEnabled && 
                        IsAlive && 
                        (PauseSystem.Instance == null || !PauseSystem.Instance.IsPaused);
        }
        
        private void HandleMovement()
        {
            Vector2 input = GetMovementInput();
            Vector3 moveDirection = new Vector3(input.x, 0f, input.y).normalized;
            
            if (moveDirection.magnitude >= MOVEMENT_THRESHOLD)
            {
                Vector3 targetPosition = transform.position + moveDirection;
                _agent.SetDestination(targetPosition);
            }
            else
            {
                _agent.ResetPath();
            }
        }
        
        private Vector2 GetMovementInput()
        {
            if (MobileInputHandler.Instance != null)
            {
                return MobileInputHandler.Instance.GetMovementInput();
            }
            
            return new Vector2(
                UnityInput.GetAxisRaw("Horizontal"), 
                UnityInput.GetAxisRaw("Vertical")
            );
        }
        
        public void SetControlEnabled(bool enabled)
        {
            _controlEnabled = enabled;
            if (_agent != null)
            {
                _agent.isStopped = !enabled;
            }
        }
        
        public void TakeDamage(float damage)
        {
            if (!IsAlive) return;
            
            _currentHealth = Mathf.Max(0, _currentHealth - damage);
            
            PlayHitEffects();
            UpdateHealthUI();
            
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            
            if (_currentHealth <= 0)
            {
                Die();
            }
        }
        
        private void PlayHitEffects()
        {
            _audioSource?.PlayOneShot(_hitSound);
            _damageFlash?.Flash();
            _animationController?.PlayTakeDamageAnimation();
        }
        
        private void UpdateHealthUI()
        {
            if (_healthBar != null)
            {
                _healthBar.UpdateHealth(_currentHealth, _maxHealth);
            }
        }
        
        public void Shoot()
        {
            if (!CanShoot()) return;
            
            EnemyController target = FindClosestEnemy();
            if (target == null) return;
            
            _lastAttackTime = Time.time;
            
            LookAtTarget(target.transform);
            PlayShootEffects();
            SpawnProjectile(target.transform.position);
        }
        
        private bool CanShoot()
        {
            return Time.time - _lastAttackTime >= _attackCooldown;
        }
        
        private void LookAtTarget(Transform target)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0f;
            
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
        
        private void PlayShootEffects()
        {
            _weapon?.PlayShootSound();
            _attackEffect?.PlayAttackAnimation();
        }
        
        private void SpawnProjectile(Vector3 targetPosition)
        {
            if (_projectilePrefab == null) return;
            
            Vector3 direction = (targetPosition - transform.position).normalized;
            direction.y = 0f;
            
            Vector3 spawnPosition = transform.position + Vector3.up + direction * _projectileSpawnOffset;
            GameObject projectileObj = Instantiate(_projectilePrefab, spawnPosition, Quaternion.identity);
            
            Projectile projectile = projectileObj.GetComponent<Projectile>();
            projectile?.Initialize(direction, _projectileSpeed, _attackDamage, true);
        }
        
        private EnemyController FindClosestEnemy()
        {
            return EnemyRegistry.Instance?.FindClosestEnemy(transform.position);
        }
        
        private void Die()
        {
            _controlEnabled = false;
            
            _audioSource?.PlayOneShot(_deathSound);
            _animationController?.PlayDeathAnimation();
            
            if (AudioService.Instance != null)
            {
                AudioService.Instance.StopMusic();
            }
            
            OnDeath?.Invoke();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other == null) return;
            
            HandleTriggerInteraction(other);
        }
        
        private void HandleTriggerInteraction(Collider other)
        {
            if (other.CompareTag("Finish"))
            {
                OnReachedFinish?.Invoke();
                return;
            }
            
            if (other.CompareTag("Obstacle"))
            {
                Die();
                return;
            }
            
            RoadController road = other.GetComponent<RoadController>();
            if (road != null)
            {
                EnterRoad(road);
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (other == null) return;
            
            RoadController road = other.GetComponent<RoadController>();
            if (road != null && road == _currentRoad)
            {
                ExitRoad();
            }
        }
        
        private void EnterRoad(RoadController road)
        {
            _currentRoad = road;
            _currentSpeedModifier = road.PlayerSpeedModifier;
            _agent.speed = _moveSpeed * _currentSpeedModifier;
            
            if (_animator != null && road.PlayerAnimatorOverride != null)
            {
                _animator.runtimeAnimatorController = road.PlayerAnimatorOverride;
            }
        }
        
        private void ExitRoad()
        {
            _currentRoad = null;
            _currentSpeedModifier = 1f;
            _agent.speed = _moveSpeed;
            
            if (_animator != null && _defaultAnimatorController != null)
            {
                _animator.runtimeAnimatorController = _defaultAnimatorController;
            }
        }
    }
}