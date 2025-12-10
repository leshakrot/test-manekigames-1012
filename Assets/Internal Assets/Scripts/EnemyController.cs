using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;
using System.Collections.Generic;
using Game.Core;
using Game.Core.UI;

namespace Game.Core.Combat
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyController : MonoBehaviour
    {
        private const float STOPPING_DISTANCE_MULTIPLIER = 0.8f;
        private const float PROJECTILE_SPAWN_OFFSET = 0.5f;
        private const float EVADE_DURATION = 0.5f;
        private const float EVADE_DISTANCE_MULTIPLIER = 2f;
        private const float EVADE_SAMPLE_RADIUS = 3f;
        private const float SAFE_EVADE_MULTIPLIER = 0.7f;
        private const float ROTATION_SPEED = 5f;
        
        [SerializeField] private float _maxHealth = 80f;
        [SerializeField] private float _attackDamage = 15f;
        [SerializeField] private float _attackCooldown = 1.5f;
        [SerializeField] private float _attackRange = 2f;
        [SerializeField] private float _moveSpeed = 3.5f;
        [SerializeField] private float _projectileSpeed = 6f;
        [SerializeField] private float _evadeDistance = 3f;
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private HealthBar _healthBar;
        [SerializeField] private float _combatDetectionRange = 15f;
        [SerializeField] private float _destroyDelayAfterDeath = 2.5f;

        [Header("Audio")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _hitSound;
        [SerializeField] private AudioClip _deathSound;
        
        private float _currentHealth;
        private float _lastAttackTime;
        private Transform _target;
        private AttackEffect _attackEffect;
        private DamageFlash _damageFlash;
        private NavMeshAgent _agent;
        private bool _isEvading;
        private EnemyAnimationController _animationController;
        private Weapon _weapon;
        private bool _isInCombat;
        
        private readonly List<Projectile> _trackedProjectiles = new List<Projectile>();
        
        public event Action<EnemyController> OnDeath;
        public event Action<float, float> OnHealthChanged;
        
        public float Health => _currentHealth;
        public float MaxHealth => _maxHealth;
        public bool IsAlive => _currentHealth > 0;
        public bool IsInCombat => _isInCombat;
        
        public void Initialize(Transform target)
        {
            _target = target;
            _currentHealth = _maxHealth;
            
            CacheComponents();
            InitializeAgent();
            InitializeAudio();
            InitializeHealth();
            RegisterToRegistry();
        }
        
        private void CacheComponents()
        {
            _attackEffect = GetComponent<AttackEffect>();
            _damageFlash = GetComponent<DamageFlash>();
            _animationController = GetComponent<EnemyAnimationController>();
            _weapon = GetComponentInChildren<Weapon>();
            _agent = GetComponent<NavMeshAgent>();
        }
        
        private void InitializeAgent()
        {
            if (_agent != null)
            {
                _agent.enabled = true;
                _agent.speed = _moveSpeed;
                _agent.stoppingDistance = _attackRange * STOPPING_DISTANCE_MULTIPLIER;
                _agent.updateRotation = false;
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
            if (_healthBar != null)
            {
                _healthBar.Initialize(transform);
                _healthBar.UpdateHealth(_currentHealth, _maxHealth);
            }
        }
        
        private void RegisterToRegistry()
        {
            if (_animationController != null)
            {
                _animationController.Initialize(this);
            }
            
            if (EnemyRegistry.Instance != null)
            {
                EnemyRegistry.Instance.RegisterEnemy(this);
            }
        }
        
        private void Update()
        {
            if (!IsAlive || _target == null) return;
            if (PauseSystem.Instance != null && PauseSystem.Instance.IsPaused) return;
            
            UpdateCombatState();
            CheckForIncomingProjectiles();
            
            if (!_isEvading)
            {
                UpdateCombatBehavior();
            }
        }
        
        private void UpdateCombatState()
        {
            if (_target == null) return;
            
            float distanceToTargetSqr = (transform.position - _target.position).sqrMagnitude;
            _isInCombat = distanceToTargetSqr <= _combatDetectionRange * _combatDetectionRange;
        }
        
        private void UpdateCombatBehavior()
        {
            float distance = Vector3.Distance(transform.position, _target.position);
            
            if (distance <= _attackRange && Time.time - _lastAttackTime >= _attackCooldown)
            {
                Attack();
            }
            else if (distance > _attackRange)
            {
                MoveTowardsTarget();
            }
            
            LookAtTarget();
        }
        
        private void CheckForIncomingProjectiles()
        {
            CleanupDestroyedProjectiles();
            
            foreach (var projectile in _trackedProjectiles)
            {
                if (ShouldEvadeProjectile(projectile))
                {
                    Vector3 evadeDirection = (transform.position - projectile.Position).normalized;
                    StartCoroutine(EvadeCoroutine(evadeDirection));
                    break;
                }
            }
        }
        
        private void CleanupDestroyedProjectiles()
        {
            _trackedProjectiles.RemoveAll(p => p == null || !p.gameObject.activeInHierarchy);
            
            Projectile[] allProjectiles = FindObjectsByType<Projectile>(FindObjectsSortMode.None);
            
            foreach (var projectile in allProjectiles)
            {
                if (projectile != null && projectile.IsPlayerProjectile && !_trackedProjectiles.Contains(projectile))
                {
                    _trackedProjectiles.Add(projectile);
                }
            }
        }
        
        private bool ShouldEvadeProjectile(Projectile projectile)
        {
            if (projectile == null || !projectile.IsPlayerProjectile || _isEvading)
                return false;
            
            float distanceSqr = (transform.position - projectile.Position).sqrMagnitude;
            if (distanceSqr >= _evadeDistance * _evadeDistance)
                return false;
            
            float projectileSpeed = projectile.Direction.magnitude;
            float safeEvadeSpeed = _moveSpeed * SAFE_EVADE_MULTIPLIER;
            
            return safeEvadeSpeed < projectileSpeed;
        }
        
        private IEnumerator EvadeCoroutine(Vector3 evadeDirection)
        {
            _isEvading = true;
            
            evadeDirection.y = 0f;
            Vector3 evadePosition = transform.position + evadeDirection * EVADE_DISTANCE_MULTIPLIER;
            
            if (NavMesh.SamplePosition(evadePosition, out NavMeshHit hit, EVADE_SAMPLE_RADIUS, NavMesh.AllAreas))
            {
                _agent.SetDestination(hit.position);
            }
            
            yield return new WaitForSeconds(EVADE_DURATION);
            
            _isEvading = false;
        }
        
        private void MoveTowardsTarget()
        {
            if (_agent != null && _target != null)
            {
                _agent.SetDestination(_target.position);
            }
        }
        
        private void LookAtTarget()
        {
            if (_target == null) return;
            
            Vector3 direction = (_target.position - transform.position).normalized;
            direction.y = 0;
            
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, ROTATION_SPEED * Time.deltaTime);
            }
        }
        
        private void Attack()
        {
            _lastAttackTime = Time.time;
            
            LookAtTarget();
            PlayAttackEffects();
            SpawnProjectile();
        }
        
        private void PlayAttackEffects()
        {
            _animationController?.PlayShootAnimation();
            _attackEffect?.PlayAttackAnimation();
            _weapon?.PlayShootSound();
        }
        
        private void SpawnProjectile()
        {
            if (_projectilePrefab == null || _target == null) return;
            
            Vector3 directionToTarget = (_target.position - transform.position).normalized;
            directionToTarget.y = 0f;
            
            Vector3 spawnPosition = transform.position + Vector3.up + directionToTarget * PROJECTILE_SPAWN_OFFSET;
            GameObject projectileObj = Instantiate(_projectilePrefab, spawnPosition, Quaternion.identity);
            
            Projectile projectile = projectileObj.GetComponent<Projectile>();
            projectile?.Initialize(directionToTarget, _projectileSpeed, _attackDamage, false);
        }
        
        public void TakeDamage(float damage, bool isCritical = false)
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
        
        private void Die()
        {
            UnregisterFromRegistry();
            StopAgent();
            PlayDeathEffects();
            
            OnDeath?.Invoke(this);
            
            Destroy(gameObject, _destroyDelayAfterDeath);
        }
        
        private void UnregisterFromRegistry()
        {
            if (EnemyRegistry.Instance != null)
            {
                EnemyRegistry.Instance.UnregisterEnemy(this);
            }
        }
        
        private void StopAgent()
        {
            if (_agent != null)
            {
                _agent.isStopped = true;
                _agent.velocity = Vector3.zero;
            }
        }
        
        private void PlayDeathEffects()
        {
            _audioSource?.PlayOneShot(_deathSound);
            _animationController?.PlayDeathAnimation();
        }
        
        private void OnDestroy()
        {
            if (EnemyRegistry.Instance != null)
            {
                EnemyRegistry.Instance.UnregisterEnemy(this);
            }
        }
    }
}