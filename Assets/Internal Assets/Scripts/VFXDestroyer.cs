using UnityEngine;

namespace Game.Core.Effects
{
    public class VFXDestroyer : MonoBehaviour
    {
        [SerializeField] private float _lifetime = 2f;
        
        private void Start()
        {
            Destroy(gameObject, _lifetime);
        }
    }
}
