using System;
using UnityEngine;

namespace Game.Core
{
    public class PauseSystem : Singleton<PauseSystem>
    {
        private bool _isPaused;
        
        public event Action<bool> OnPauseStateChanged;
        
        public bool IsPaused => _isPaused;
        
        public void Pause()
        {
            if (_isPaused) return;
            
            SetPauseState(true);
        }
        
        public void Resume()
        {
            if (!_isPaused) return;
            
            SetPauseState(false);
        }
        
        public void TogglePause()
        {
            SetPauseState(!_isPaused);
        }
        
        private void SetPauseState(bool paused)
        {
            _isPaused = paused;
            OnPauseStateChanged?.Invoke(_isPaused);
        }
    }
}