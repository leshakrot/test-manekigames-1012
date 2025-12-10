using UnityEngine;
using Game.Core;

namespace Game.Core.Combat
{
    public class Projectile : MonoBehaviour
    {
        private const string DEFAULT_LAYER = "Default";     
        [SerializeField] private float _lifetime = 10f;
        
        private Vector3 _direction;
        private float _speed;
        private float _damage;
        private bool _isPlayerProjectile;
        private bool _isInitialized;
        
        public bool IsPlayerProjectile => _isPlayerProjectile;
        public Vector3 Position => transform.position;
        public Vector3 Direction => _direction;
        
        public void Initialize(Vector3 direction, float speed, float damage, bool isPlayerProjectile)
        {
            _direction = direction.normalized;
            _speed = speed;
            _damage = damage;
            _isPlayerProjectile = isPlayerProjectile;
            _isInitialized = true;
            
            Destroy(gameObject, _lifetime);
        }
        
        private void Update()
        {
            if (!_isInitialized) return;
            if (PauseSystem.Instance != null && PauseSystem.Instance.IsPaused) return;
            
            MoveProjectile();
        }
        
        private void MoveProjectile()
        {
            transform.position += _direction * _speed * Time.deltaTime;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other == null) return;
            
            if (TryHitTarget(other) || IsEnvironmentCollision(other))
            {
                DestroyProjectile();
            }
        }
        
        private bool TryHitTarget(Collider other)
        {
            if (_isPlayerProjectile)
            {
                return TryHitEnemy(other);
            }
            else
            {
                return TryHitPlayer(other);
            }
        }
        
        private bool TryHitEnemy(Collider other)
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            
            if (enemy != null && enemy.IsAlive)
            {
                enemy.TakeDamage(_damage);
                return true;
            }
            
            return false;
        }
        
        private bool TryHitPlayer(Collider other)
        {
            Player.PlayerController player = other.GetComponent<Player.PlayerController>();
            
            if (player != null && player.IsAlive)
            {
                player.TakeDamage(_damage);
                return true;
            }
            
            return false;
        }
        
        private bool IsEnvironmentCollision(Collider other)
        {
            return other.gameObject.layer == LayerMask.NameToLayer(DEFAULT_LAYER) && 
                   !other.isTrigger;
        }
        
        private void DestroyProjectile()
        {
            Destroy(gameObject);
        }
    }
}