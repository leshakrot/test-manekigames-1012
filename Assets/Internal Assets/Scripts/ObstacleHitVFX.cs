using UnityEngine;

namespace Game.Core.Effects
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ObstacleHitVFX : MonoBehaviour
    {
        [SerializeField] private float _lifetime = 2f;
        
        private ParticleSystem _particleSystem;
        
        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            
            var main = _particleSystem.main;
            main.stopAction = ParticleSystemStopAction.Destroy;
        }
        
        private void Start()
        {
            if (_particleSystem != null)
            {
                _particleSystem.Play();
            }
            
            Destroy(gameObject, _lifetime);
        }
    }
}
