using UnityEngine;
using System;
using System.Collections.Generic;

namespace Game.Core.Puzzle
{
    public enum KeyColor
    {
        Red,
        Blue,
        Green,
        Yellow
    }
    
    public class PuzzleController
    {
        [SerializeField] private int _gridSize = 6;
        [SerializeField] private int _requiredKeysNumber = 3;
        
        private KeyColor _lockColor;
        private List<KeyColor> _keys;
        private int _correctKeysPlaced;
        
        public event Action OnPuzzleCompleted;
        
        public KeyColor LockColor => _lockColor;
        public List<KeyColor> Keys => _keys;
        public int CorrectKeysCount => _correctKeysPlaced;
        public int RequiredKeys => _requiredKeysNumber;
        
        public void GeneratePuzzle()
        {
            _lockColor = (KeyColor)UnityEngine.Random.Range(0, 4);
            _correctKeysPlaced = 0;
            
            _keys = new List<KeyColor>();
            
            for (int i = 0; i < _requiredKeysNumber; i++)
            {
                _keys.Add(_lockColor);
            }
            
            int totalKeys = _gridSize * _gridSize;
            for (int i = _requiredKeysNumber; i < totalKeys; i++)
            {
                KeyColor randomColor;
                do
                {
                    randomColor = (KeyColor)UnityEngine.Random.Range(0, 4);
                } while (randomColor == _lockColor && UnityEngine.Random.value > 0.3f);
                
                _keys.Add(randomColor);
            }
            
            for (int i = 0; i < _keys.Count; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, _keys.Count);
                KeyColor temp = _keys[i];
                _keys[i] = _keys[randomIndex];
                _keys[randomIndex] = temp;
            }
        }
        
        public bool TryPlaceKey(KeyColor keyColor)
        {
            if (keyColor == _lockColor)
            {
                _correctKeysPlaced++;
                
                if (_correctKeysPlaced >= _requiredKeysNumber)
                {
                    OnPuzzleCompleted?.Invoke();
                }
                
                return true;
            }
            
            return false;
        }
        
        public Color GetColorForKey(KeyColor keyColor)
        {
            switch (keyColor)
            {
                case KeyColor.Red: return Color.red;
                case KeyColor.Blue: return Color.blue;
                case KeyColor.Green: return Color.green;
                case KeyColor.Yellow: return Color.yellow;
                default: return Color.white;
            }
        }
    }
}