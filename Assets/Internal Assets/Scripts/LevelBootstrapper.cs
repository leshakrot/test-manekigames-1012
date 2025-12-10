using UnityEngine;
using System.Collections;
using Game.Core.Player;
using Game.Core.Obstacles;
using Game.Core.Combat;
using Game.Core.Puzzle;
using Game.Core.UI;
using Game.Core.MobileInput;
using Game.Core.Audio;

namespace Game.Bootstrap
{
    public class LevelBootstrapper : MonoBehaviour
    {
        private const float DEFEAT_PANEL_DELAY = 1f;
        
        [Header("Spawn Points")]
        [SerializeField] private Transform _startPoint;
        [SerializeField] private Transform _finishPoint;
        [SerializeField] private Transform[] _enemySpawnPoints;
        
        [Header("Roads")]
        [SerializeField] private RoadController[] _roads;

        [Header("Prefabs")]
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField] private GameObject _chestPrefab;
        
        [Header("UI")]
        [SerializeField] private UIController _uiController;
        [SerializeField] private MobileInputHandler _inputHandler;
        
        private PlayerController _player;
        private GameStateController _gameState;
        private CombatController _combat;
        private PuzzleController _puzzle;
        private bool _enemiesSpawned;
        
        public void Initialize()
        {
            InitializeSystems();
            InitializeLevel();
            SetupEventHandlers();
        }
        
        private void InitializeSystems()
        {
            EnsureRegistryExists();
            
            _gameState = new GameStateController();
            _puzzle = new PuzzleController();
            
            if (_inputHandler != null)
            {
                _inputHandler.Initialize(_gameState);
            }
            
            if (AudioService.Instance != null)
            {
                AudioService.Instance.PlayMusic();
            }
        }
        
        private void EnsureRegistryExists()
        {
            if (EnemyRegistry.Instance == null)
            {
                var registryObj = new GameObject("EnemyRegistry");
                registryObj.AddComponent<EnemyRegistry>();
            }
        }
        
        private void InitializeLevel()
        {
            SpawnPlayer();
            InitializeRoads();
            
            if (_uiController != null)
            {
                _uiController.Initialize(_puzzle);
            }
        }
        
        private void SetupEventHandlers()
        {
            if (_player != null)
            {
                _player.OnReachedFinish += OnPlayerReachedFinish;
                _player.OnDeath += OnPlayerDeath;
            }
            
            if (_uiController != null)
            {
                _uiController.OnRestart += RestartLevel;
            }
        }
        
        private void SpawnPlayer()
        {
            if (_playerPrefab == null || _startPoint == null) return;
            
            GameObject playerObj = Instantiate(_playerPrefab, _startPoint.position, Quaternion.identity);
            _player = playerObj.GetComponent<PlayerController>();
            _player.Initialize();
            
            PlayerCombatInput combatInput = playerObj.GetComponent<PlayerCombatInput>();
            combatInput?.Initialize(_gameState);
        }
        
        private void InitializeRoads()
        {
            if (_roads == null) return;
            
            foreach (RoadController road in _roads)
            {
                if (road != null)
                {
                    road.Initialize();
                    road.OnPlayerHit += OnPlayerDeath;
                }
            }
        }
        
        private void OnPlayerReachedFinish()
        {
            if (_enemiesSpawned) return;
            
            _enemiesSpawned = true;
            _player.SetControlEnabled(false);
            
            SpawnEnemies();
        }
        
        private void SpawnEnemies()
        {
            if (_enemySpawnPoints == null || _enemySpawnPoints.Length == 0) return;

            EnemyController[] enemies = new EnemyController[_enemySpawnPoints.Length];

            for (int i = 0; i < _enemySpawnPoints.Length; i++)
            {
                enemies[i] = CreateEnemy(_enemySpawnPoints[i]);
            }
            
            InitializeCombat(enemies);
        }
        
        private EnemyController CreateEnemy(Transform spawnPoint)
        {
            GameObject enemyObj = Instantiate(_enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            
            EnemyController controller = enemyObj.GetComponent<EnemyController>();
            controller.Initialize(_player.transform);
            
            EnemyAI ai = enemyObj.GetComponent<EnemyAI>();
            ai?.Initialize(_player.transform);
            
            return controller;
        }
        
        private void InitializeCombat(EnemyController[] enemies)
        {
            _combat = new CombatController();
            _combat.Initialize(_player, enemies, _gameState);
            _combat.OnPlayerDefeated += OnPlayerDeath;
            _combat.OnEnemiesDefeated += OnEnemiesDefeated;
            _combat.StartCombat();
        }
        
        private void OnEnemiesDefeated(Vector3 lastEnemyPosition)
        {
            if (_chestPrefab == null) return;
            
            GameObject chest = Instantiate(_chestPrefab, lastEnemyPosition, Quaternion.identity);
            ChestController chestController = chest.GetComponent<ChestController>();
            
            if (chestController != null)
            {
                chestController.OnOpened += OnChestOpened;
            }
        }
        
        private void OnChestOpened()
        {
            if (_uiController != null)
            {
                _uiController.ShowPuzzle();
            }
            
            if (_puzzle != null)
            {
                _puzzle.OnPuzzleCompleted += OnPuzzleCompleted;
            }
        }
        
        private void OnPuzzleCompleted()
        {
            if (_uiController != null)
            {
                _uiController.ShowVictory();
            }
        }
        
        private void OnPlayerDeath()
        {
            _player?.SetControlEnabled(false);
            StartCoroutine(ShowDefeatAfterDelay());
        }
        
        private IEnumerator ShowDefeatAfterDelay()
        {
            yield return new WaitForSeconds(DEFEAT_PANEL_DELAY);
            
            if (_uiController != null)
            {
                _uiController.ShowDefeat();
            }
        }
        
        private void RestartLevel()
        {
            if (EnemyRegistry.Instance != null)
            {
                EnemyRegistry.Instance.ClearAllEnemies();
            }
            
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
        }
        
        private void OnDestroy()
        {
            CleanupEventHandlers();
        }
        
        private void CleanupEventHandlers()
        {
            if (_player != null)
            {
                _player.OnReachedFinish -= OnPlayerReachedFinish;
                _player.OnDeath -= OnPlayerDeath;
            }
            
            if (_uiController != null)
            {
                _uiController.OnRestart -= RestartLevel;
            }
            
            if (_roads != null)
            {
                foreach (RoadController road in _roads)
                {
                    if (road != null)
                    {
                        road.OnPlayerHit -= OnPlayerDeath;
                    }
                }
            }
        }
    }
}