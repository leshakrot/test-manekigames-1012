using UnityEngine;
using System;
using System.Collections.Generic;
using Game.Bootstrap;

namespace Game.Core.Combat
{
    /// <summary>
    /// Manages combat state and coordinates between player and enemies
    /// </summary>
    public class CombatController
    {
        private Player.PlayerController _player;
        private readonly List<EnemyController> _enemies = new List<EnemyController>();
        private GameStateController _gameState;
        
        private bool _combatActive;
        private Vector3 _lastEnemyDeathPosition;
        
        public event Action OnPlayerDefeated;
        public event Action<Vector3> OnEnemiesDefeated;
        
        public bool IsCombatActive => _combatActive;
        public int RemainingEnemies => _enemies.Count;
        
        public void Initialize(Player.PlayerController player, EnemyController[] enemies, GameStateController gameState)
        {
            _player = player;
            _gameState = gameState;
            
            AddEnemies(enemies);
            SubscribeToPlayerEvents();
        }
        
        private void AddEnemies(EnemyController[] enemies)
        {
            _enemies.Clear();
            
            foreach (EnemyController enemy in enemies)
            {
                if (enemy != null)
                {
                    _enemies.Add(enemy);
                    enemy.Initialize(_player.transform);
                    enemy.OnDeath += HandleEnemyDeath;
                }
            }
        }
        
        private void SubscribeToPlayerEvents()
        {
            if (_player != null)
            {
                _player.OnDeath += HandlePlayerDeath;
            }
        }
        
        public void StartCombat()
        {
            _combatActive = true;
            _player?.SetControlEnabled(true);
            
            SetGameState(GameStateController.GameState.Combat);
        }
        
        public void EndCombat()
        {
            _combatActive = false;
            SetGameState(GameStateController.GameState.Moving);
        }
        
        private void SetGameState(GameStateController.GameState newState)
        {
            if (_gameState != null)
            {
                _gameState.SetState(newState);
            }
        }
        
        private void HandleEnemyDeath(EnemyController enemy)
        {
            if (enemy == null) return;
            
            UnsubscribeFromEnemy(enemy);
            _lastEnemyDeathPosition = enemy.transform.position;
            _enemies.Remove(enemy);
            
            CheckForVictory();
        }
        
        private void UnsubscribeFromEnemy(EnemyController enemy)
        {
            enemy.OnDeath -= HandleEnemyDeath;
        }
        
        private void CheckForVictory()
        {
            if (_enemies.Count == 0 && _combatActive)
            {
                EndCombat();
                OnEnemiesDefeated?.Invoke(_lastEnemyDeathPosition);
            }
        }
        
        private void HandlePlayerDeath()
        {
            if (_combatActive)
            {
                _combatActive = false;
                OnPlayerDefeated?.Invoke();
            }
        }
        
        public void Cleanup()
        {
            UnsubscribeFromPlayer();
            UnsubscribeFromAllEnemies();
            
            _enemies.Clear();
            _combatActive = false;
        }
        
        private void UnsubscribeFromPlayer()
        {
            if (_player != null)
            {
                _player.OnDeath -= HandlePlayerDeath;
            }
        }
        
        private void UnsubscribeFromAllEnemies()
        {
            foreach (EnemyController enemy in _enemies)
            {
                if (enemy != null)
                {
                    enemy.OnDeath -= HandleEnemyDeath;
                }
            }
        }
    }
}