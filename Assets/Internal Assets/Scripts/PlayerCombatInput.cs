using UnityEngine;
using Game.Core;
using Game.Core.Combat;
using Game.Bootstrap;

namespace Game.Core.Player
{
    public class PlayerCombatInput : MonoBehaviour
    {
        [SerializeField] private GameStateController _gameStateController;
        
        private PlayerController _player;
        private EnemyController _currentTarget;
        
        private void Start()
        {
            _player = GetComponent<PlayerController>();
        }

        public void Initialize(GameStateController gameStateController)
        {
            _gameStateController = gameStateController;
        }
        
        private void Update()
        {
            if (!_player.IsAlive) return;
            if (PauseSystem.Instance != null && PauseSystem.Instance.IsPaused) return;
            
            UpdateTargeting();
            
            if (_gameStateController != null && _gameStateController.CurrentState == GameStateController.GameState.Combat)
            {
                TryShoot();
            }
        }
        
        private void UpdateTargeting()
        {
            _currentTarget = null;
            
            if (EnemyRegistry.Instance != null)
            {
                _currentTarget = EnemyRegistry.Instance.FindClosestEnemy(transform.position);
            }
        }
        
        private void TryShoot()
        {
            if (_currentTarget != null && _currentTarget.IsAlive)
            {
                _player.Shoot();
            }
        }
    }
}