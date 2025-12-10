using UnityEngine;
using UnityEngine.UI;

namespace Game.Core.UI
{
    public class HealthBar : MonoBehaviour
    {
        private const float MIN_FILL = 0f;
        private const float MAX_FILL = 1f;
        
        [SerializeField] private Image _fillImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private float _smoothSpeed = 5f;
        [SerializeField] private Vector3 _offset = new Vector3(0, 2.5f, 0);
        
        [Header("Colors")]
        [SerializeField] private Color _fullHealthColor = Color.green;
        [SerializeField] private Color _lowHealthColor = Color.red;
        
        private Transform _target;
        private Camera _mainCamera;
        
        private float _currentFillAmount = MAX_FILL;
        private float _targetFillAmount = MAX_FILL;
        private RectTransform _fillRectTransform;
        private float _maxWidth;
        
        public void Initialize(Transform target)
        {
            _target = target;
            _mainCamera = Camera.main;
            
            CacheComponents();
            ResetFill();
        }
        
        private void CacheComponents()
        {
            if (_fillImage != null)
            {
                _fillRectTransform = _fillImage.GetComponent<RectTransform>();
                if (_fillRectTransform != null)
                {
                    _maxWidth = _fillRectTransform.rect.width;
                }
            }
        }
        
        private void ResetFill()
        {
            _currentFillAmount = MAX_FILL;
            _targetFillAmount = MAX_FILL;
        }
        
        public void UpdateHealth(float currentHealth, float maxHealth)
        {
            _targetFillAmount = CalculateFillAmount(currentHealth, maxHealth);
        }
        
        private float CalculateFillAmount(float current, float max)
        {
            if (max <= 0f) return MIN_FILL;
            return Mathf.Clamp01(current / max);
        }
        
        private void Update()
        {
            if (_target == null) return;
            
            UpdatePosition();
            UpdateRotation();
            UpdateFillAmount();
        }
        
        private void UpdatePosition()
        {
            transform.position = _target.position + _offset;
        }
        
        private void UpdateRotation()
        {
            if (_mainCamera == null) return;
            
            Vector3 directionToCamera = transform.position - _mainCamera.transform.position;
            transform.rotation = Quaternion.LookRotation(directionToCamera);
        }
        
        private void UpdateFillAmount()
        {
            if (_fillImage == null || _fillRectTransform == null) return;
            
            _currentFillAmount = Mathf.Lerp(
                _currentFillAmount, 
                _targetFillAmount, 
                _smoothSpeed * Time.deltaTime
            );
            
            UpdateVisuals();
        }
        
        private void UpdateVisuals()
        {
            float newWidth = _maxWidth * _currentFillAmount;
            _fillRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
            
            _fillImage.color = Color.Lerp(_lowHealthColor, _fullHealthColor, _currentFillAmount);
        }
        
        public void Show()
        {
            gameObject.SetActive(true);
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        public void SetVisibility(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}