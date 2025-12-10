using UnityEngine;

namespace Game.Core.Combat
{
    public class ProjectileVisualizer : MonoBehaviour
    {
        [Header("Visual Settings")]
        [SerializeField] private TrailRenderer _trailRenderer;
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private Light _projectileLight;
        
        private void Start()
        {
            if (_trailRenderer == null)
            {
                _trailRenderer = GetComponent<TrailRenderer>();
            }
            
            if (_meshRenderer == null)
            {
                _meshRenderer = GetComponentInChildren<MeshRenderer>();
            }
            
            if (_projectileLight == null)
            {
                _projectileLight = GetComponentInChildren<Light>();
            }
        }
        
        public void SetColor(Color color)
        {
            if (_trailRenderer != null)
            {
                _trailRenderer.startColor = color;
                _trailRenderer.endColor = new Color(color.r, color.g, color.b, 0f);
            }
            
            if (_meshRenderer != null)
            {
                _meshRenderer.material.color = color;
            }
            
            if (_projectileLight != null)
            {
                _projectileLight.color = color;
            }
        }
    }
}
