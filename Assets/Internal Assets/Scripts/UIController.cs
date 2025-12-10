using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Game.Core;

namespace Game.Core.UI
{
    public class UIController : MonoBehaviour
    {       
        [Header("Puzzle Panel")]
        [SerializeField] private GameObject _puzzlePanel;
        [SerializeField] private Image _lockImage;
        [SerializeField] private TextMeshProUGUI _keyCounterText;
        [SerializeField] private Transform _keysGrid;
        [SerializeField] private GameObject _keyPrefab;
        [SerializeField] private Transform _lockDropZone;
        
        [Header("End Game Panel")]
        [SerializeField] private GameObject _endGamePanel;
        [SerializeField] private TextMeshProUGUI _endGameText;
        [SerializeField] private Button _restartButton;
        [SerializeField] private string _victoryText;
        [SerializeField] private string _defeatText;

        private Puzzle.PuzzleController _puzzle;
        
        public event Action OnRestart;
        
        public void Initialize(Puzzle.PuzzleController puzzle)
        {
            _puzzle = puzzle;
            
            _puzzlePanel.SetActive(false);
            _endGamePanel.SetActive(false);
            
            _restartButton.onClick.AddListener(() => OnRestart?.Invoke());
        }
        
        public void ShowPuzzle()
        {
            _puzzle.GeneratePuzzle();
            _puzzlePanel.SetActive(true);
            
            SetupLock();
            SetupKeys();
            UpdateKeyCounter();
            
            if (PauseSystem.Instance != null)
            {
                PauseSystem.Instance.Pause();
            }
        }
        
        private void SetupLock()
        {
            _lockImage.color = _puzzle.GetColorForKey(_puzzle.LockColor);
        }
        
        private void SetupKeys()
        {
            foreach (Transform child in _keysGrid)
            {
                Destroy(child.gameObject);
            }
            
            for (int i = 0; i < _puzzle.Keys.Count; i++)
            {
                var keyObj = Instantiate(_keyPrefab, _keysGrid);
                var keyUI = keyObj.GetComponent<KeyUI>();
                keyUI.Initialize(_puzzle.Keys[i], _puzzle, this);
            }
        }
        
        public void UpdateKeyCounter()
        {
            _keyCounterText.text = $"{_puzzle.CorrectKeysCount} / {_puzzle.RequiredKeys}";
        }
        
        public void ShowVictory()
        {
            _puzzlePanel.SetActive(false);
            ShowEndGame(true);
            
            if (PauseSystem.Instance != null)
            {
                PauseSystem.Instance.Resume();
            }
        }
        
        public void ShowDefeat()
        {
            _puzzlePanel.SetActive(false);
            ShowEndGame(false);
            
            if (PauseSystem.Instance != null)
            {
                PauseSystem.Instance.Resume();
            }
        }
        
        private void ShowEndGame(bool victory)
        {
            _endGamePanel.SetActive(true);

            string textKey = victory ? _victoryText : _defeatText;
            _endGameText.text = textKey;
        }

        private void OnDestroy()
        {
            if (_restartButton != null)
            {
                _restartButton.onClick.RemoveAllListeners();
            }
        }
    }
}