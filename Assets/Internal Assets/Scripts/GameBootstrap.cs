using UnityEngine;

namespace Game.Bootstrap
{
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private LevelBootstrapper _levelBootstrapper;
        
        private void Awake()
        {
            if (_levelBootstrapper == null) return;

            _levelBootstrapper.Initialize();
        }
    }
}