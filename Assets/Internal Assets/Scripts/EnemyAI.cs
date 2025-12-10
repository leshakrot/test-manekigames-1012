using UnityEngine;
using Game.Core;

namespace Game.Core.Combat
{
    public enum EnemyBehavior
    {
        Aggressive,
        Defensive,
        Flanking
    }
    public class EnemyAI : MonoBehaviour
    {
        private const float DEGREES_TO_RADIANS = Mathf.Deg2Rad;
        private const float DEFENSIVE_MOVE_MULTIPLIER = 0.5f;
        
        [SerializeField] private EnemyBehavior _behavior = EnemyBehavior.Aggressive;
        [SerializeField] private float _moveSpeed = 2f;
        [SerializeField] private float _minDistance = 1.5f;
        [SerializeField] private float _maxDistance = 4f;
        [SerializeField] private float _circleSpeed = 2f;
        
        private Transform _target;
        private EnemyController _controller;
        private float _circleAngle;
        private float _optimalFlankingDistance;
        
        public void Initialize(Transform target)
        {
            _target = target;
            _controller = GetComponent<EnemyController>();
            
            _circleAngle = Random.Range(0f, 360f);
            _optimalFlankingDistance = (_minDistance + _maxDistance) / 2f;
        }
        
        private void Update()
        {
            if (!CanUpdate()) return;
            
            float distanceToTarget = Vector3.Distance(transform.position, _target.position);
            ExecuteBehavior(distanceToTarget);
        }
        
        private bool CanUpdate()
        {
            return _target != null && 
                   _controller != null && 
                   _controller.IsAlive &&
                   (PauseSystem.Instance == null || !PauseSystem.Instance.IsPaused);
        }
        
        private void ExecuteBehavior(float distance)
        {
            switch (_behavior)
            {
                case EnemyBehavior.Aggressive:
                    AggressiveBehavior(distance);
                    break;
                    
                case EnemyBehavior.Defensive:
                    DefensiveBehavior(distance);
                    break;
                    
                case EnemyBehavior.Flanking:
                    FlankingBehavior();
                    break;
            }
        }
        
        private void AggressiveBehavior(float distance)
        {
            if (distance > _minDistance)
            {
                MoveTowardsTarget();
            }
        }
        
        private void DefensiveBehavior(float distance)
        {
            if (distance < _minDistance)
            {
                MoveAwayFromTarget();
            }
            else if (distance > _maxDistance)
            {
                MoveTowardsTarget(DEFENSIVE_MOVE_MULTIPLIER);
            }
        }
        
        private void FlankingBehavior()
        {
            _circleAngle += _circleSpeed * Time.deltaTime;
            
            Vector3 circleOffset = CalculateCircleOffset();
            Vector3 desiredPosition = _target.position + circleOffset;
            
            MoveToPosition(desiredPosition);
        }
        
        private Vector3 CalculateCircleOffset()
        {
            float angleInRadians = _circleAngle * DEGREES_TO_RADIANS;
            
            return new Vector3(
                Mathf.Cos(angleInRadians), 
                0f, 
                Mathf.Sin(angleInRadians)
            ) * _optimalFlankingDistance;
        }
        
        private void MoveTowardsTarget(float speedMultiplier = 1f)
        {
            Vector3 direction = (_target.position - transform.position).normalized;
            Move(direction, speedMultiplier);
        }
        
        private void MoveAwayFromTarget()
        {
            Vector3 direction = (transform.position - _target.position).normalized;
            Move(direction);
        }
        
        private void MoveToPosition(Vector3 targetPosition)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            Move(direction);
        }
        
        private void Move(Vector3 direction, float speedMultiplier = 1f)
        {
            transform.position += direction * _moveSpeed * speedMultiplier * Time.deltaTime;
        }
    }
}