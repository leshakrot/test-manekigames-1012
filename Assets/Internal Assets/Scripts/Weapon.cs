using UnityEngine;

namespace Game.Core.Combat
{
    public class Weapon : MonoBehaviour
    {       
        [Header("Audio")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _shootSound;
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;
        [SerializeField, Range(0f, 1f)] private float _spatialBlend = 0f;
        
        private void Awake()
        {
            InitializeAudioSource();
        }
        
        private void InitializeAudioSource()
        {
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
            }
            
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            ConfigureAudioSource();
        }
        
        private void ConfigureAudioSource()
        {
            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = _spatialBlend;
            _audioSource.rolloffMode = AudioRolloffMode.Linear;
        }
        
        public void PlayShootSound()
        {
            if (_audioSource == null)
            {
                InitializeAudioSource();
            }
            
            if (_shootSound != null)
            {
                _audioSource.PlayOneShot(_shootSound, _volume);
            }
        }
        
        public void SetVolume(float volume)
        {
            _volume = Mathf.Clamp01(volume);
        }
        
        public void SetSpatialBlend(float spatialBlend)
        {
            _spatialBlend = Mathf.Clamp01(spatialBlend);
            
            if (_audioSource != null)
            {
                _audioSource.spatialBlend = _spatialBlend;
            }
        }
    }
}