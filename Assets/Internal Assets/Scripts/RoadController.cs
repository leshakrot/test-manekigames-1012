using UnityEngine;
using System;
using System.Collections.Generic;
using Game.Core;

namespace Game.Core.Obstacles
{
    [Serializable]
    public class ObstacleType
    {
        public GameObject prefab;
        [Range(0.5f, 2f)] public float speedMultiplier = 1f;
    }
    
    public enum MovementDirection
    {
        LeftToRight,
        RightToLeft,
        Both
    }
    
    public class RoadController : MonoBehaviour
    {
        private const float OBSTACLE_CLEANUP_BUFFER = 5f;
        private const float HALF_DIVISOR = 2f;
        
        [Header("Road Settings")]
        [SerializeField] private float _roadLength = 50f;
        [SerializeField] private float _laneOffset = 2f;
        [SerializeField, Range(0.1f, 2f)] private float _playerSpeedModifier = 1f;
        [SerializeField] private AnimatorOverrideController _playerAnimatorOverride;
        
        [Header("Obstacle Settings")]
        [SerializeField] private List<ObstacleType> _obstacleTypes = new List<ObstacleType>();
        [SerializeField] private float _baseObstacleSpeed = 3f;
        [SerializeField] private float _spawnInterval = 2f;
        [SerializeField] private int _maxObstacles = 5;
        [SerializeField] private float _obstacleRotationY = 180f;
        [SerializeField] private float _obstaclePositionRandomOffset = 1f;
        [SerializeField] private MovementDirection _movementDirection = MovementDirection.Both;
        
        private readonly List<ObstacleController> _activeObstacles = new List<ObstacleController>();
        private readonly Queue<ObstacleController> _obstaclePool = new Queue<ObstacleController>();
        
        private float _nextSpawnTime;
        private float _halfRoadLength;
        private float _maxDistanceFromCenter;
        
        public event Action OnPlayerHit;
        
        public float PlayerSpeedModifier => _playerSpeedModifier;
        public AnimatorOverrideController PlayerAnimatorOverride => _playerAnimatorOverride;
        
        public void Initialize()
        {
            CacheCalculations();
            InitializePool();
            
            _nextSpawnTime = Time.time + _spawnInterval;
        }
        
        private void CacheCalculations()
        {
            _halfRoadLength = _roadLength / HALF_DIVISOR;
            _maxDistanceFromCenter = _halfRoadLength + OBSTACLE_CLEANUP_BUFFER;
        }
        
        private void InitializePool()
        {
            if (_obstacleTypes == null || _obstacleTypes.Count == 0) return;

            int obstaclesPerType = Mathf.Max(1, _maxObstacles / _obstacleTypes.Count);

            foreach (ObstacleType obstacleType in _obstacleTypes)
            {
                if (obstacleType.prefab == null) continue;

                CreatePooledObstacles(obstacleType.prefab, obstaclesPerType);
            }
        }
        
        private void CreatePooledObstacles(GameObject prefab, int count)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject obstacleObj = Instantiate(prefab, transform);
                ObstacleController obstacle = obstacleObj.GetComponent<ObstacleController>();

                if (obstacle == null)
                {
                    Destroy(obstacleObj);
                    continue;
                }

                obstacleObj.SetActive(false);
                _obstaclePool.Enqueue(obstacle);
            }
        }
        
        private void Update()
        {
            if (PauseSystem.Instance != null && PauseSystem.Instance.IsPaused) 
                return;
            
            if (ShouldSpawnObstacle())
            {
                SpawnObstacle();
                _nextSpawnTime = Time.time + _spawnInterval;
            }
            
            UpdateObstacles();
        }
        
        private bool ShouldSpawnObstacle()
        {
            return Time.time >= _nextSpawnTime && 
                   _activeObstacles.Count < _maxObstacles && 
                   _obstaclePool.Count > 0;
        }
        
        private void SpawnObstacle()
        {
            ObstacleController obstacle = _obstaclePool.Dequeue();
            ObstacleType selectedType = GetRandomObstacleType();
            
            bool moveRight = DetermineMovementDirection();
            ConfigureObstacle(obstacle, selectedType, moveRight);
            
            _activeObstacles.Add(obstacle);
        }
        
        private ObstacleType GetRandomObstacleType()
        {
            return _obstacleTypes[UnityEngine.Random.Range(0, _obstacleTypes.Count)];
        }
        
        private void ConfigureObstacle(ObstacleController obstacle, ObstacleType type, bool moveRight)
        {
            Vector3 direction = moveRight ? Vector3.right : Vector3.left;
            Vector3 spawnPosition = CalculateSpawnPosition(moveRight);
            Quaternion rotation = moveRight ? Quaternion.identity : Quaternion.Euler(0f, _obstacleRotationY, 0f);
            
            obstacle.transform.position = spawnPosition;
            obstacle.transform.rotation = rotation;
            obstacle.gameObject.SetActive(true);
            
            float finalSpeed = _baseObstacleSpeed * type.speedMultiplier;
            obstacle.Initialize(finalSpeed, direction);
            obstacle.OnPlayerCollision += HandlePlayerCollision;
        }
        
        private Vector3 CalculateSpawnPosition(bool moveRight)
        {
            Vector3 position = transform.position;
            
            float horizontalOffset = moveRight ? -_halfRoadLength : _halfRoadLength;
            float forwardOffset = moveRight ? _laneOffset : -_laneOffset;
            
            position += Vector3.right * horizontalOffset;
            position += Vector3.forward * forwardOffset;
            position.x += UnityEngine.Random.Range(-_obstaclePositionRandomOffset, _obstaclePositionRandomOffset);
            
            return position;
        }
        
        private bool DetermineMovementDirection()
        {
            switch (_movementDirection)
            {
                case MovementDirection.LeftToRight:
                    return true;
                    
                case MovementDirection.RightToLeft:
                    return false;
                    
                case MovementDirection.Both:
                default:
                    return UnityEngine.Random.value > 0.5f;
            }
        }
        
        private void UpdateObstacles()
        {
            for (int i = _activeObstacles.Count - 1; i >= 0; i--)
            {
                ObstacleController obstacle = _activeObstacles[i];
                
                if (IsObstacleOutOfBounds(obstacle))
                {
                    ReturnObstacleToPool(obstacle, i);
                }
            }
        }
        
        private bool IsObstacleOutOfBounds(ObstacleController obstacle)
        {
            float distanceFromCenter = Mathf.Abs(obstacle.transform.position.x - transform.position.x);
            return distanceFromCenter > _maxDistanceFromCenter;
        }
        
        private void ReturnObstacleToPool(ObstacleController obstacle, int index)
        {
            obstacle.OnPlayerCollision -= HandlePlayerCollision;
            _activeObstacles.RemoveAt(index);
            
            obstacle.gameObject.SetActive(false);
            _obstaclePool.Enqueue(obstacle);
        }
        
        private void HandlePlayerCollision()
        {
            OnPlayerHit?.Invoke();
        }
        
        private void OnDestroy()
        {
            CleanupObstacles();
        }
        
        private void CleanupObstacles()
        {
            foreach (ObstacleController obstacle in _activeObstacles)
            {
                if (obstacle != null)
                {
                    obstacle.OnPlayerCollision -= HandlePlayerCollision;
                }
            }
            
            _activeObstacles.Clear();
        }
    }
}