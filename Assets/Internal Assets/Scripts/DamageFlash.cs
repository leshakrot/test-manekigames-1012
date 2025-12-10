using UnityEngine;

namespace Game.Core.Combat
{
    public class DamageFlash : MonoBehaviour
    {
        private const float FULL_FLASH = 1f;
        
        [SerializeField] private Color _flashColor = Color.red;
        [SerializeField] private float _flashDuration = 0.2f;
        
        private MeshRenderer[] _renderers;
        private Color[] _originalColors;
        private Material[] _materials;
        
        private float _flashTimer;
        private bool _isFlashing;
        
        private void Start()
        {
            CacheRenderersAndColors();
        }
        
        private void CacheRenderersAndColors()
        {
            _renderers = GetComponentsInChildren<MeshRenderer>();
            _originalColors = new Color[_renderers.Length];
            _materials = new Material[_renderers.Length];
            
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null && _renderers[i].material != null)
                {
                    _materials[i] = _renderers[i].material;
                    _originalColors[i] = _materials[i].color;
                }
            }
        }
        
        public void Flash()
        {
            _isFlashing = true;
            _flashTimer = 0f;
        }
        
        private void Update()
        {
            if (!_isFlashing) return;
            
            _flashTimer += Time.deltaTime;
            
            if (_flashTimer < _flashDuration)
            {
                UpdateFlashEffect();
            }
            else
            {
                EndFlash();
            }
        }
        
        private void UpdateFlashEffect()
        {
            float flashIntensity = CalculateFlashIntensity();
            ApplyFlashToRenderers(flashIntensity);
        }
        
        private float CalculateFlashIntensity()
        {
            return FULL_FLASH - (_flashTimer / _flashDuration);
        }
        
        private void ApplyFlashToRenderers(float intensity)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_materials[i] != null)
                {
                    _materials[i].color = Color.Lerp(_originalColors[i], _flashColor, intensity);
                }
            }
        }
        
        private void EndFlash()
        {
            _isFlashing = false;
            ResetColorsToOriginal();
        }
        
        private void ResetColorsToOriginal()
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_materials[i] != null)
                {
                    _materials[i].color = _originalColors[i];
                }
            }
        }
        
        private void OnDestroy()
        {
            if (_materials != null)
            {
                foreach (Material material in _materials)
                {
                    if (material != null)
                    {
                        Destroy(material);
                    }
                }
            }
        }
    }
}