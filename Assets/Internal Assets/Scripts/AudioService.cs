using UnityEngine;

namespace Game.Core.Audio
{
    public class AudioService : Singleton<AudioService>
    {
        [Header("Music")]
        [SerializeField] private AudioSource _musicSource;
        
        [Header("Settings")]
        [SerializeField, Range(0f, 1f)] private float _musicVolume = 0.7f;
        
        protected override void Awake()
        {
            base.Awake();
            
            if (_musicSource != null)
            {
                _musicSource.volume = _musicVolume;
            }
        }
        
        public void PlayMusic()
        {
            if (_musicSource != null && !_musicSource.isPlaying)
            {
                _musicSource.Play();
            }
        }
        
        public void StopMusic()
        {
            if (_musicSource != null)
            {
                _musicSource.Stop();
            }
        }
        
        public void PauseMusic()
        {
            if (_musicSource != null && _musicSource.isPlaying)
            {
                _musicSource.Pause();
            }
        }
        
        public void ResumeMusic()
        {
            if (_musicSource != null && !_musicSource.isPlaying)
            {
                _musicSource.UnPause();
            }
        }
        
        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            if (_musicSource != null)
            {
                _musicSource.volume = _musicVolume;
            }
        }
        
        public void FadeOutMusic(float duration = 1f)
        {
            if (_musicSource != null)
            {
                StartCoroutine(FadeOutCoroutine(duration));
            }
        }
        
        private System.Collections.IEnumerator FadeOutCoroutine(float duration)
        {
            float startVolume = _musicSource.volume;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }
            
            _musicSource.Stop();
            _musicSource.volume = startVolume;
        }
    }
}