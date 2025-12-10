using UnityEngine;
using System;
using Game.Core;

namespace Game.Core.Obstacles
{
    [RequireComponent(typeof(Collider))]
    public class ObstacleController : MonoBehaviour
    {
        private const string PLAYER_TAG = "Player";
        
        [Header("Effects")]
        [SerializeField] private GameObject _hitVFXPrefab;
        [SerializeField] private AudioClip _hitSound;
        [SerializeField, Range(0f, 1f)] private float _hitSoundVolume = 1f;
        
        private float _speed;
        private Vector3 _direction;
        private AudioSource _audioSource;
        private bool _hasCollided;
        
        public event Action OnPlayerCollision;
        
        private void Awake()
        {
            InitializeAudioSource();
            ConfigureCollider();
        }
        
        private void InitializeAudioSource()
        {
            _audioSource = GetComponent<AudioSource>();
            
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            _audioSource.playOnAwake = false;
        }
        
        private void ConfigureCollider()
        {
            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }
        
        public void Initialize(float speed, Vector3 direction)
        {
            _speed = speed;
            _direction = direction.normalized;
            _hasCollided = false;
            
            gameObject.tag = "Obstacle";
        }
        
        private void Update()
        {
            if (PauseSystem.Instance != null && PauseSystem.Instance.IsPaused) 
                return;
            
            MoveObstacle();
        }
        
        private void MoveObstacle()
        {
            transform.position += _direction * _speed * Time.deltaTime;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other == null || _hasCollided) return;
            
            if (other.CompareTag(PLAYER_TAG))
            {
                HandlePlayerCollision(other.transform.position);
            }
        }
        
        private void HandlePlayerCollision(Vector3 hitPosition)
        {
            _hasCollided = true;
            
            PlayHitEffects(hitPosition);
            OnPlayerCollision?.Invoke();
        }
        
        private void PlayHitEffects(Vector3 position)
        {
            SpawnVFX(position);
            PlayHitSound();
        }
        
        private void SpawnVFX(Vector3 position)
        {
            if (_hitVFXPrefab != null)
            {
                Instantiate(_hitVFXPrefab, position, Quaternion.identity);
            }
        }
        
        private void PlayHitSound()
        {
            if (_hitSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(_hitSound, _hitSoundVolume);
            }
        }
        
        private void OnDisable()
        {
            _hasCollided = false;
        }
    }
}